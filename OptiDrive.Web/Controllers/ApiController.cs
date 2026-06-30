using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;
using System.Globalization;
using System.Text;

namespace OptiDrive.Web.Controllers;

[ApiController]
[Route("api")]
public sealed class ApiController(
    ExternalVehicleCatalogService vehicleCatalog,
    ExternalFuelStationService fuelStations,
    ExternalElectricStationService electricStations,
    GeocodingService geocodingService,
    AppStateService appState,
    SmartSaveService smartSave) : ControllerBase
{
    [HttpGet("catalog/makes")]
    public async Task<IActionResult> Makes(CancellationToken cancellationToken)
    {
        var makes = await vehicleCatalog.GetMakesAsync(cancellationToken);
        return Ok(new { count = makes.Count, results = makes });
    }

    [HttpGet("catalog/models")]
    public async Task<IActionResult> Models([FromQuery] string make, CancellationToken cancellationToken)
    {
        var models = await vehicleCatalog.GetModelsAsync(make, cancellationToken);
        return Ok(new { make, count = models.Count, results = models });
    }

    [HttpGet("fuel/stations")]
    public async Task<IActionResult> Stations([FromQuery] string? fuelKind, [FromQuery] string? city, [FromQuery] string? brand, CancellationToken cancellationToken)
    {
        var stations = await fuelStations.GetAllStationsAsync(false, cancellationToken);
        var evStations = await electricStations.GetElectricStationsAsync(cancellationToken);
        var filtered = stations.Concat(evStations)
            .GroupBy(station => string.IsNullOrWhiteSpace(station.ExternalId) ? station.Name + "|" + station.Latitude + "|" + station.Longitude : station.ExternalId)
            .Select(group => group.First())
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(fuelKind) &&
            Enum.TryParse<FuelKind>(fuelKind, true, out var parsedFuel))
        {
            filtered = filtered.Where(station => OffersFuel(station, parsedFuel));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = Normalize(city);
            filtered = filtered.Where(station => Normalize(station.City).Contains(normalizedCity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            filtered = filtered.Where(station => station.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase));
        }

        var results = filtered
            .Take(500)
            .Select(station => new
            {
                station.Name,
                station.Brand,
                station.City,
                station.District,
                station.Address,
                FuelKind = station.FuelKind.ToString(),
                station.Price,
                station.AvailableFuels,
                station.AvailableFuelKinds,
                station.FuelPriceSummary,
                station.Latitude,
                station.Longitude,
                station.IsElectricCharging,
                station.LastUpdatedAt,
                station.Source
            })
            .ToList();

        return Ok(new { count = results.Count, results });
    }

    [HttpGet("fuel/types")]
    public async Task<IActionResult> FuelTypes(CancellationToken cancellationToken)
    {
        var fuelTypes = await fuelStations.GetFuelTypesAsync(cancellationToken);
        var results = fuelTypes.Select(item => new
        {
            item.Id,
            item.Description
        });

        return Ok(new { count = fuelTypes.Count, results });
    }

    [HttpGet("status/sync")]
    public IActionResult SyncStatus()
        => Ok(appState.SyncStatuses());

    [HttpGet("planning/places")]
    public IActionResult PlanningPlaces()
        => Ok(smartSave.KnownPlaces.Select(item => new
        {
            name = item.Key,
            latitude = item.Value.Latitude,
            longitude = item.Value.Longitude
        }));

    [HttpGet("planning/search-places")]
    public async Task<IActionResult> SearchPlaces([FromQuery] string q, CancellationToken cancellationToken)
    {
        var results = await geocodingService.SearchAsync(q, 6, cancellationToken);
        return Ok(new
        {
            count = results.Count,
            results = results.Select(item => new
            {
                label = item.DisplayName,
                value = item.DisplayName,
                item.Latitude,
                item.Longitude,
                item.Provider
            })
        });
    }

    [HttpGet("planning/map-data")]
    public async Task<IActionResult> PlanningMapData([FromQuery] Guid? routeId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        var liquidStations = await fuelStations.GetAllStationsAsync(false, cancellationToken);
        var evStations = await electricStations.GetElectricStationsAsync(cancellationToken);
        var dashboard = userId.HasValue ? appState.BuildDashboard(userId.Value) : null;
        var route = dashboard?.Routes.OrderByDescending(item => item.CreatedAt).FirstOrDefault()
                    ?? appState.AllRoutes().OrderByDescending(item => item.CreatedAt).FirstOrDefault();
        if (routeId.HasValue)
        {
            route = dashboard?.Routes.FirstOrDefault(item => item.Id == routeId.Value)
                    ?? appState.AllRoutes().FirstOrDefault(item => item.Id == routeId.Value)
                    ?? route;
        }

        var selectedVehicle = dashboard?.Vehicles.FirstOrDefault(vehicle => vehicle.Id == route?.VehicleId)
                              ?? dashboard?.Vehicles.FirstOrDefault(vehicle => vehicle.IsDefault)
                              ?? dashboard?.Vehicles.FirstOrDefault();
        var selectedFuelKind = selectedVehicle?.FuelKind;
        var routePoints = route is null ? [] : smartSave.ReadRoutePoints(route);
        var suggestedStops = route is null ? [] : smartSave.ReadSuggestedStops(route);
        var routeRelevantStations = liquidStations
            .Concat(evStations)
            .GroupBy(station => string.IsNullOrWhiteSpace(station.ExternalId) ? station.Name + "|" + station.Latitude + "|" + station.Longitude : station.ExternalId)
            .Select(group => group.First());

        if (routePoints.Count > 1)
        {
            routeRelevantStations = routeRelevantStations
                .Select(station => new
                {
                    Station = station,
                    DistanceToRouteKm = DistanceToRoute(station, routePoints)
                })
                .Where(item => item.DistanceToRouteKm <= 12)
                .OrderBy(item => item.DistanceToRouteKm)
                .ThenBy(item => item.Station.Price <= 0 ? double.MaxValue : item.Station.Price)
                .Select(item => item.Station);
        }

        var stations = routeRelevantStations
            .OrderByDescending(station => station.IsElectricCharging)
            .ThenBy(station => station.Price <= 0 ? double.MaxValue : station.Price)
            .ThenBy(station => station.Name)
            .Take(routePoints.Count > 1 ? 600 : 1600)
            .Select(station => new
            {
                station.Name,
                station.Brand,
                station.City,
                station.District,
                station.Address,
                fuelKind = station.FuelKind.ToString(),
                station.Price,
                station.AvailableFuels,
                station.AvailableFuelKinds,
                station.FuelPriceSummary,
                station.FuelPriceMapJson,
                selectedFuelPrice = selectedFuelKind.HasValue ? GetPriceForFuel(station, selectedFuelKind.Value) : station.Price,
                station.Latitude,
                station.Longitude,
                station.IsLowCost,
                station.IsElectricCharging,
                station.Source
            });

        var routePath = route is null ? [] : smartSave.BuildRoutePath(route);
        var brandList = liquidStations
            .Concat(evStations)
            .Select(station => station.Brand)
            .Where(brandName => !string.IsNullOrWhiteSpace(brandName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(brandName => brandName)
            .Take(60)
            .ToList();

        return Ok(new
        {
            route = route is null
                ? null
                : new
                {
                    route.Id,
                    route.VehicleId,
                    route.Origin,
                    route.Destination,
                    route.Waypoints,
                    route.AvoidTolls,
                    route.DistanceKm,
                    route.EtaMinutes,
                    route.FastestCost,
                    route.CheapestCost,
                    route.TollCost,
                    route.EstimatedConsumptionAmount,
                    route.AverageSpeedKmh,
                    route.AdjustedConsumptionPer100,
                    route.ConsumptionSpeedFactor,
                    route.MinimumArrivalLevelPercent,
                    route.TotalSuggestedReplenishmentAmount,
                    route.EstimatedArrivalLevelPercent,
                    route.EstimatedDirectArrivalLevelPercent,
                    route.SuggestedStopCount,
                    route.RequiresIntermediateStops,
                    route.CarbonKg,
                    route.Recommendation,
                    route.CriticalAlert,
                    route.SavingsScore,
                    route.RecommendedStationName,
                    route.RecommendedChargeAmount
                },
            routePath,
            suggestedStops = suggestedStops.Select((stop, index) => new
            {
                id = index + 1,
                stop.StationName,
                stop.Brand,
                stop.City,
                fuelKind = stop.FuelKind.ToString(),
                stop.Price,
                stop.ProgressKm,
                stop.SuggestedAmount,
                stop.Latitude,
                stop.Longitude
            }),
            selectedVehicle = selectedVehicle is null
                ? null
                : new
                {
                    selectedVehicle.Id,
                    selectedVehicle.Nickname,
                    fuelKind = selectedVehicle.FuelKind.ToString(),
                    selectedVehicle.CurrentLevelPercent,
                    selectedVehicle.AverageConsumption,
                    selectedVehicle.AverageRangeKm,
                    selectedVehicle.TankCapacity,
                    selectedVehicle.BatteryCapacity
                },
            brands = brandList,
            stations
        });
    }

    private static bool OffersFuel(FuelStation station, FuelKind fuelKind)
    {
        var available = station.AvailableFuelKinds
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return available.Length == 0
            ? station.FuelKind == fuelKind
            : available.Contains(fuelKind.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private static double GetPriceForFuel(FuelStation station, FuelKind fuelKind)
    {
        if (!string.IsNullOrWhiteSpace(station.FuelPriceMapJson))
        {
            try
            {
                var prices = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(station.FuelPriceMapJson);
                if (prices is not null && prices.TryGetValue(fuelKind.ToString(), out var value))
                {
                    return value;
                }
            }
            catch
            {
            }
        }

        return station.FuelKind == fuelKind ? station.Price : 0;
    }

    private static double DistanceToRoute(FuelStation station, IReadOnlyList<GeocodedPlace> routePoints)
    {
        return routePoints
            .Select(point => HaversineKm(point.Latitude, point.Longitude, station.Latitude, station.Longitude))
            .DefaultIfEmpty(double.MaxValue)
            .Min();
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        var radius = 6371d;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double angle) => Math.PI * angle / 180d;

    private static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
