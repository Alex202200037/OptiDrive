using System.Globalization;
using System.Text.Json;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class SmartSaveService(
    GeocodingService geocodingService,
    GoogleDirectionsService googleDirectionsService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private sealed record RouteStationCandidate(
        string StationKey,
        FuelStation Station,
        double Distance,
        double Price,
        double ProgressKm);
    private static readonly Dictionary<string, (double Latitude, double Longitude)> SuggestedPlaces = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Lisboa"] = (38.7223, -9.1393),
        ["Setubal"] = (38.5244, -8.8882),
        ["Palmela"] = (38.5690, -8.9010),
        ["Almada"] = (38.6790, -9.1569),
        ["Grandola"] = (38.1770, -8.5660),
        ["Alcacer do Sal"] = (38.3733, -8.5144),
        ["Evora"] = (38.5710, -7.9135),
        ["Beja"] = (38.0151, -7.8632),
        ["Faro"] = (37.0194, -7.9304),
        ["Portimao"] = (37.1366, -8.5378)
    };

    public IReadOnlyList<string> PlaceNames => SuggestedPlaces.Keys.Order().ToList();
    public IReadOnlyDictionary<string, (double Latitude, double Longitude)> KnownPlaces => SuggestedPlaces;

    public async Task<PlannedRoute> BuildRouteAsync(Guid ownerId, VehicleProfile vehicle, RoutePlanInputModel input, IReadOnlyList<FuelStation> stations, CancellationToken cancellationToken = default)
    {
        var waypointQueries = (input.Waypoints ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        GoogleRouteResult? googleRoute = null;
        try
        {
            googleRoute = await googleDirectionsService.GetDrivingRouteAsync(
                input.Origin ?? string.Empty,
                input.Destination ?? string.Empty,
                waypointQueries,
                input.AvoidTolls,
                cancellationToken);
        }
        catch
        {
        }

        var routePoints = googleRoute?.Path?.Count > 1
            ? googleRoute.Path.ToList()
            : await ResolveRoutePointsAsync(input, cancellationToken);
        if (routePoints.Count < 2)
        {
            throw new InvalidOperationException("Nao foi possivel resolver origem e destino.");
        }

        var distanceKm = googleRoute?.DistanceKm ?? 0d;
        if (distanceKm <= 0)
        {
            for (var index = 0; index < routePoints.Count - 1; index++)
            {
                distanceKm += HaversineKm(routePoints[index], routePoints[index + 1]) * 1.18;
            }
        }

        var eta = googleRoute?.DurationMinutes
                  ?? (int)Math.Round((distanceKm / Math.Max(60, input.AverageSpeed)) * 60);
        var adjustedConsumption = AdjustConsumptionForSpeed(vehicle, input.AverageSpeed);
        var speedFactor = adjustedConsumption / Math.Max(0.1, vehicle.AverageConsumption);
        var effectiveRangeKm = vehicle.AverageRangeKm > 0
            ? vehicle.AverageRangeKm / speedFactor
            : 0;
        var baseEnergy = distanceKm / 100d * adjustedConsumption;
        var tollCost = input.AvoidTolls ? 0 : Math.Round(distanceKm * TollMultiplier(vehicle.TollClass), 2);
        var cumulativeKm = BuildCumulativeDistances(routePoints);
        var routeStations = stations
            .Where(station => StationOffersFuel(station, vehicle.FuelKind))
            .Select(station => new RouteStationCandidate(
                BuildStationKey(station),
                station,
                DistanceToRoute(station, routePoints),
                GetPriceForFuel(station, vehicle.FuelKind),
                GetProgressKm(station, routePoints, cumulativeKm)))
            .Where(item => item.Distance <= 18)
            .OrderBy(item => item.ProgressKm)
            .ToList();

        var stationCandidates = routeStations.Count > 0
            ? routeStations
                .OrderBy(item => item.Price <= 0 ? double.MaxValue : item.Price)
                .ThenBy(item => item.Distance)
                .Take(80)
                .Select(item => item.Station)
                .ToList()
            : stations
                .Where(station => StationOffersFuel(station, vehicle.FuelKind))
                .OrderBy(station => GetPriceForFuel(station, vehicle.FuelKind) <= 0 ? double.MaxValue : GetPriceForFuel(station, vehicle.FuelKind))
                .Take(80)
                .ToList();

        var pricedStations = stationCandidates
            .Select(station => new { Station = station, Price = GetPriceForFuel(station, vehicle.FuelKind) })
            .Where(item => item.Price > 0)
            .ToList();
        var averagePrice = pricedStations.Count > 0
            ? pricedStations.Average(station => station.Price)
            : vehicle.FuelKind == FuelKind.Electric ? 0.49 : 1.70;

        var recommended = pricedStations.FirstOrDefault()?.Station ?? stationCandidates.FirstOrDefault();
        var recommendedPrice = recommended is null ? 0 : GetPriceForFuel(recommended, vehicle.FuelKind);
        var optimizedFuelCost = baseEnergy * (recommendedPrice > 0 ? recommendedPrice : averagePrice);
        var capacity = vehicle.FuelKind == FuelKind.Electric ? vehicle.BatteryCapacity ?? 0 : vehicle.TankCapacity;
        var autonomyKm = effectiveRangeKm * vehicle.CurrentLevelPercent / 100d;
        var minimumArrivalLevelPercent = MinimumArrivalLevelPercent(vehicle);
        var stopPlans = BuildStopPlans(vehicle, distanceKm, autonomyKm, effectiveRangeKm, adjustedConsumption, routeStations);
        var totalSuggestedReplenishment = stopPlans.Sum(stop => Math.Max(0, stop.SuggestedAmount));
        var currentAmount = capacity > 0 ? capacity * vehicle.CurrentLevelPercent / 100d : 0;
        var directArrivalPercent = capacity > 0
            ? Math.Clamp((int)Math.Round((currentAmount - baseEnergy) / capacity * 100d), 0, 100)
            : 0;
        var arrivalPercent = capacity > 0
            ? Math.Clamp((int)Math.Round((currentAmount + totalSuggestedReplenishment - baseEnergy) / capacity * 100d), 0, 100)
            : 0;
        if (capacity > 0 && stopPlans.Count > 0 && arrivalPercent < minimumArrivalLevelPercent)
        {
            for (var stopIndex = stopPlans.Count - 1; stopIndex >= 0 && arrivalPercent < minimumArrivalLevelPercent; stopIndex--)
            {
                var missingAmount = capacity * (minimumArrivalLevelPercent - arrivalPercent) / 100d;
                var stop = stopPlans[stopIndex];
                var extraAmount = Math.Min(missingAmount, Math.Max(0, capacity - stop.SuggestedAmount));
                if (extraAmount <= 0)
                {
                    continue;
                }

                stop.SuggestedAmount = Math.Round(stop.SuggestedAmount + extraAmount, 1);
                totalSuggestedReplenishment = stopPlans.Sum(item => Math.Max(0, item.SuggestedAmount));
                arrivalPercent = Math.Clamp((int)Math.Round((currentAmount + totalSuggestedReplenishment - baseEnergy) / capacity * 100d), 0, 100);
            }
        }

        var canFinishDirect = autonomyKm >= distanceKm + ReserveKm(vehicle, effectiveRangeKm);
        var recommendation = stopPlans.Count > 0
            ? $"Autonomia insuficiente para viagem direta. Smart Save planeou {stopPlans.Count} paragem(ns): {string.Join(", ", stopPlans.Take(3).Select(stop => $"{stop.StationName} ({stop.ProgressKm:0} km)"))}."
            : recommended is null
                ? "Nenhum posto compatível encontrado ao longo da rota."
                : $"Parar em {recommended.Name} ({recommended.City}) para aproveitar {BuildStationPriceLabel(recommended, vehicle.FuelKind)}.";
        var criticalAlert = stopPlans.Count > 0 && arrivalPercent < minimumArrivalLevelPercent
            ? $"Alerta: paragens planeadas, mas a margem estimada ({arrivalPercent}%) fica abaixo do alvo de {minimumArrivalLevelPercent}%."
            : !canFinishDirect && stopPlans.Count == 0
            ? "Alerta crítico: autonomia insuficiente e não foi encontrado um plano de paragens compatível."
            : stopPlans.Count > 0
                ? $"Viagem requer {stopPlans.Count} paragem(ns) intermédia(s) para garantir margem de segurança."
                : "Autonomia adequada para a viagem com margem de segurança.";
        var leadStop = stopPlans.FirstOrDefault();

        return new PlannedRoute
        {
            OwnerId = ownerId,
            VehicleId = vehicle.Id,
            Origin = googleRoute?.OriginAddress ?? routePoints[0].DisplayName,
            Destination = googleRoute?.DestinationAddress ?? routePoints[^1].DisplayName,
            Waypoints = string.Join(", ", waypointQueries),
            AvoidTolls = input.AvoidTolls,
            OriginLatitude = routePoints[0].Latitude,
            OriginLongitude = routePoints[0].Longitude,
            DestinationLatitude = routePoints[^1].Latitude,
            DestinationLongitude = routePoints[^1].Longitude,
            RoutePathJson = JsonSerializer.Serialize(routePoints, JsonOptions),
            DistanceKm = Math.Round(distanceKm, 1),
            EtaMinutes = eta,
            FastestCost = Math.Round(baseEnergy * averagePrice + tollCost, 2),
            CheapestCost = Math.Round(optimizedFuelCost + tollCost, 2),
            TollCost = tollCost,
            CarbonKg = Math.Round(vehicle.FuelKind == FuelKind.Electric ? baseEnergy * 0.12 : baseEnergy * 2.31, 1),
            EstimatedConsumptionAmount = Math.Round(baseEnergy, 1),
            AverageSpeedKmh = input.AverageSpeed,
            AdjustedConsumptionPer100 = Math.Round(adjustedConsumption, 2),
            ConsumptionSpeedFactor = Math.Round(speedFactor, 3),
            MinimumArrivalLevelPercent = minimumArrivalLevelPercent,
            TotalSuggestedReplenishmentAmount = Math.Round(totalSuggestedReplenishment, 1),
            EstimatedArrivalLevelPercent = arrivalPercent,
            EstimatedDirectArrivalLevelPercent = directArrivalPercent,
            SuggestedStopCount = stopPlans.Count,
            RequiresIntermediateStops = stopPlans.Count > 0,
            SavingsScore = Math.Clamp((int)Math.Round(7 + ((baseEnergy * averagePrice) - optimizedFuelCost)), 1, 10),
            Recommendation = recommendation,
            CriticalAlert = criticalAlert,
            RecommendedStationName = leadStop?.StationName ?? recommended?.Name ?? "Sem sugestão",
            RecommendedChargeAmount = Math.Round(leadStop?.SuggestedAmount ?? Math.Min(baseEnergy, vehicle.FuelKind == FuelKind.Electric ? vehicle.BatteryCapacity ?? 60 : vehicle.TankCapacity * 0.6), 1),
            SuggestedStopsJson = JsonSerializer.Serialize(stopPlans, JsonOptions)
        };
    }

    public IReadOnlyList<object> BuildRoutePath(PlannedRoute route)
    {
        var points = ReadRoutePoints(route);
        return points.Select(point => new
        {
            name = point.DisplayName,
            latitude = point.Latitude,
            longitude = point.Longitude
        }).Cast<object>().ToList();
    }

    public IReadOnlyList<GeocodedPlace> ReadRoutePoints(PlannedRoute route)
    {
        if (string.IsNullOrWhiteSpace(route.RoutePathJson))
        {
            return [];
        }

        try
        {
            var points = JsonSerializer.Deserialize<List<GeocodedPlace>>(route.RoutePathJson, JsonOptions);
            return points is { Count: > 0 } ? points : [];
        }
        catch
        {
            return [];
        }
    }

    public IReadOnlyList<RouteStopPlan> ReadSuggestedStops(PlannedRoute route)
    {
        if (string.IsNullOrWhiteSpace(route.SuggestedStopsJson))
        {
            return [];
        }

        try
        {
            var stops = JsonSerializer.Deserialize<List<RouteStopPlan>>(route.SuggestedStopsJson, JsonOptions);
            return stops is { Count: > 0 } ? stops : [];
        }
        catch
        {
            return [];
        }
    }

    private async Task<List<GeocodedPlace>> ResolveRoutePointsAsync(RoutePlanInputModel input, CancellationToken cancellationToken)
    {
        var waypointQueries = (input.Waypoints ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var queries = waypointQueries
            .Prepend(input.Origin ?? string.Empty)
            .Append(input.Destination ?? string.Empty)
            .ToList();
        var points = new List<GeocodedPlace>();

        foreach (var query in queries)
        {
            var point = await ResolvePlaceAsync(query, cancellationToken);
            if (point is not null)
            {
                points.Add(point);
            }
        }

        return points;
    }

    private async Task<GeocodedPlace?> ResolvePlaceAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        if (SuggestedPlaces.TryGetValue(query, out var known))
        {
            return new GeocodedPlace
            {
                Query = query,
                DisplayName = query,
                Latitude = known.Latitude,
                Longitude = known.Longitude,
                Provider = "OptiDrive Places"
            };
        }

        return await geocodingService.GeocodeAsync(query, cancellationToken);
    }

    private static bool StationOffersFuel(FuelStation station, FuelKind fuelKind)
    {
        var values = station.AvailableFuelKinds
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return values.Length == 0
            ? station.FuelKind == fuelKind
            : values.Contains(fuelKind.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildStationPriceLabel(FuelStation station, FuelKind fuelKind)
    {
        var price = GetPriceForFuel(station, fuelKind);
        return price > 0
            ? $"{FuelLabel(fuelKind)} {price.ToString("0.000", CultureInfo.InvariantCulture)} EUR"
            : station.FuelPriceSummary;
    }

    private static double GetPriceForFuel(FuelStation station, FuelKind fuelKind)
    {
        try
        {
            var map = JsonSerializer.Deserialize<Dictionary<string, double>>(station.FuelPriceMapJson, JsonOptions);
            if (map is not null && map.TryGetValue(fuelKind.ToString(), out var value))
            {
                return value;
            }
        }
        catch
        {
        }

        return station.FuelKind == fuelKind ? station.Price : 0;
    }

    private static string FuelLabel(FuelKind fuelKind)
        => fuelKind switch
        {
            FuelKind.Gasoline95 => "Gasolina 95",
            FuelKind.Gasoline98 => "Gasolina 98",
            FuelKind.Diesel => "Gasoleo",
            FuelKind.GPL => "GPL",
            FuelKind.Electric => "Eletricidade",
            _ => fuelKind.ToString()
        };

    private static string BuildStationKey(FuelStation station)
    {
        if (!string.IsNullOrWhiteSpace(station.ExternalId))
        {
            return station.ExternalId;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{station.Name}|{station.Brand}|{station.Latitude:0.00000}|{station.Longitude:0.00000}");
    }

    private static List<double> BuildCumulativeDistances(IReadOnlyList<GeocodedPlace> routePoints)
    {
        var cumulative = new List<double> { 0 };
        var total = 0d;
        for (var index = 1; index < routePoints.Count; index++)
        {
            total += HaversineKm(routePoints[index - 1], routePoints[index]);
            cumulative.Add(total);
        }

        return cumulative;
    }

    private static double GetProgressKm(FuelStation station, IReadOnlyList<GeocodedPlace> routePoints, IReadOnlyList<double> cumulativeKm)
    {
        var bestIndex = 0;
        var bestDistance = double.MaxValue;
        for (var index = 0; index < routePoints.Count; index++)
        {
            var distance = HaversineKm((routePoints[index].Latitude, routePoints[index].Longitude), (station.Latitude, station.Longitude));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = index;
            }
        }

        return cumulativeKm.ElementAtOrDefault(bestIndex);
    }

    private static List<RouteStopPlan> BuildStopPlans(
        VehicleProfile vehicle,
        double totalDistanceKm,
        double autonomyKm,
        double effectiveRangeKm,
        double adjustedConsumptionPer100Km,
        IReadOnlyList<RouteStationCandidate> routeStations)
    {
        var reserveKm = ReserveKm(vehicle, effectiveRangeKm);
        var initialReachKm = Math.Max(0, autonomyKm - reserveKm);
        var refillReachKm = Math.Max(50, effectiveRangeKm - reserveKm);
        var stops = new List<RouteStopPlan>();
        var capacity = vehicle.FuelKind == FuelKind.Electric
            ? vehicle.BatteryCapacity ?? 60
            : vehicle.TankCapacity;

        if (capacity <= 0 || initialReachKm >= totalDistanceKm || routeStations.Count == 0)
        {
            return stops;
        }

        var currentProgress = 0d;
        var reachableKm = initialReachKm;
        var usedStations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (currentProgress + reachableKm < totalDistanceKm && stops.Count < 6)
        {
            var maxProgress = currentProgress + reachableKm;
            var targetProgress = currentProgress + reachableKm * 0.75;
            var minProgress = currentProgress + Math.Max(25, reachableKm * 0.35);

            var candidates = routeStations
                .Where(item => item.ProgressKm > currentProgress + 5
                               && item.ProgressKm <= maxProgress
                               && item.ProgressKm >= minProgress
                               && !usedStations.Contains(item.StationKey))
                .OrderBy(item => Math.Abs(item.ProgressKm - targetProgress))
                .ThenBy(item => item.Price <= 0 ? double.MaxValue : item.Price)
                .ThenBy(item => item.Distance)
                .Take(10)
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = routeStations
                    .Where(item => item.ProgressKm > currentProgress + 5
                                   && item.ProgressKm <= maxProgress
                                   && !usedStations.Contains(item.StationKey))
                    .OrderByDescending(item => item.ProgressKm)
                    .ThenBy(item => item.Price <= 0 ? double.MaxValue : item.Price)
                    .Take(10)
                    .ToList();
            }

            var candidate = candidates.FirstOrDefault();
            if (candidate is null || candidate.ProgressKm <= currentProgress + 1)
            {
                break;
            }

            var remainingDistance = Math.Max(0, totalDistanceKm - candidate.ProgressKm);
            var nextLegKm = Math.Min(refillReachKm, remainingDistance + reserveKm);
            var minimumTopUp = vehicle.FuelKind == FuelKind.Electric
                ? capacity * 0.30
                : capacity * 0.22;
            var suggestedAmount = Math.Min(
                capacity,
                Math.Max(nextLegKm / 100d * adjustedConsumptionPer100Km * 1.10, minimumTopUp));

            stops.Add(new RouteStopPlan
            {
                StationName = candidate.Station.Name,
                Brand = candidate.Station.Brand,
                City = candidate.Station.City,
                FuelKind = vehicle.FuelKind,
                Price = candidate.Price,
                ProgressKm = Math.Round(candidate.ProgressKm, 0),
                SuggestedAmount = Math.Round(suggestedAmount, 1),
                Latitude = candidate.Station.Latitude,
                Longitude = candidate.Station.Longitude
            });

            usedStations.Add(candidate.StationKey);
            currentProgress = candidate.ProgressKm;
            reachableKm = refillReachKm;
        }

        return stops;
    }

    private static double AdjustConsumptionForSpeed(VehicleProfile vehicle, int averageSpeed)
    {
        var speed = Math.Clamp(averageSpeed <= 0 ? 90 : averageSpeed, 35, 150);
        var baseConsumption = Math.Max(0.1, vehicle.AverageConsumption);

        double factor;
        if (vehicle.FuelKind == FuelKind.Electric)
        {
            factor = speed switch
            {
                <= 50 => 1.10,
                <= 80 => 0.95,
                <= 100 => 1.00,
                <= 120 => 1.00 + ((speed - 100) * 0.010),
                _ => 1.20 + ((speed - 120) * 0.012)
            };
        }
        else
        {
            factor = speed switch
            {
                <= 50 => 1.14,
                <= 80 => 0.96,
                <= 100 => 1.00,
                <= 120 => 1.00 + ((speed - 100) * 0.006),
                _ => 1.12 + ((speed - 120) * 0.010)
            };
        }

        return Math.Round(baseConsumption * factor, 2);
    }

    private static int MinimumArrivalLevelPercent(VehicleProfile vehicle)
        => vehicle.FuelKind == FuelKind.Electric ? 12 : 8;

    private static double ReserveKm(VehicleProfile vehicle, double effectiveRangeKm)
    {
        var referenceRange = effectiveRangeKm > 0 ? effectiveRangeKm : vehicle.AverageRangeKm;
        return vehicle.FuelKind == FuelKind.Electric
            ? Math.Clamp(referenceRange * 0.12, 25, 60)
            : Math.Clamp(referenceRange * 0.08, 20, 45);
    }

    private static double DistanceToRoute(FuelStation station, IReadOnlyList<GeocodedPlace> routePoints)
    {
        return routePoints
            .Select(point => HaversineKm((point.Latitude, point.Longitude), (station.Latitude, station.Longitude)))
            .DefaultIfEmpty(double.MaxValue)
            .Min();
    }

    private static double TollMultiplier(TollClass tollClass)
        => tollClass switch
        {
            TollClass.Class1 => 0.075,
            TollClass.Class2 => 0.105,
            TollClass.Class3 => 0.130,
            TollClass.Class4 => 0.155,
            _ => 0.075
        };

    private static double HaversineKm(GeocodedPlace first, GeocodedPlace second)
        => HaversineKm((first.Latitude, first.Longitude), (second.Latitude, second.Longitude));

    private static double HaversineKm((double Latitude, double Longitude) first, (double Latitude, double Longitude) second)
    {
        var radius = 6371d;
        var deltaLat = ToRadians(second.Latitude - first.Latitude);
        var deltaLon = ToRadians(second.Longitude - first.Longitude);
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(ToRadians(first.Latitude)) * Math.Cos(ToRadians(second.Latitude)) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double angle) => Math.PI * angle / 180d;
}
