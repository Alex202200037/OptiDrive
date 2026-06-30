using Microsoft.Extensions.Configuration;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;
using Xunit;

namespace OptiDrive.Web.Tests;

public sealed class SmartSaveServiceTests
{
    [Fact]
    public async Task BuildRouteAsync_WithEnoughAutonomy_RecommendsStationWithoutIntermediateStops()
    {
        var smartSave = CreateSmartSaveService();
        var vehicle = new VehicleProfile
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Nickname = "Touring Diesel",
            Brand = "Peugeot",
            Model = "308 SW",
            Year = 2021,
            FuelKind = FuelKind.Diesel,
            TollClass = TollClass.Class1,
            AverageConsumption = 5.2,
            TankCapacity = 53,
            AverageRangeKm = 940,
            CurrentLevelPercent = 72
        };

        var route = await smartSave.BuildRouteAsync(
            vehicle.OwnerId,
            vehicle,
            new RoutePlanInputModel
            {
                VehicleId = vehicle.Id,
                Origin = "Setubal",
                Destination = "Alcacer do Sal",
                AverageSpeed = 90,
                AvoidTolls = true
            },
            BuildStations());

        Assert.Equal("Setubal", route.Origin);
        Assert.Equal("Alcacer do Sal", route.Destination);
        Assert.False(route.RequiresIntermediateStops);
        Assert.Equal(0, route.SuggestedStopCount);
        Assert.Equal(0, route.TollCost);
        Assert.NotEmpty(route.Recommendation);
        Assert.NotEmpty(route.RecommendedStationName);
        Assert.True(route.DistanceKm > 0);
        Assert.Equal(90, route.AverageSpeedKmh);
        Assert.True(route.AdjustedConsumptionPer100 > 0);
    }

    [Fact]
    public async Task BuildRouteAsync_ForLowBatteryEv_PlansIntermediateStops()
    {
        var smartSave = CreateSmartSaveService();
        var vehicle = new VehicleProfile
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Nickname = "Trip EV",
            Brand = "Tesla",
            Model = "Model 3",
            Year = 2024,
            FuelKind = FuelKind.Electric,
            TollClass = TollClass.Class1,
            AverageConsumption = 15.8,
            TankCapacity = 0,
            BatteryCapacity = 60,
            AverageRangeKm = 420,
            CurrentLevelPercent = 25
        };

        var route = await smartSave.BuildRouteAsync(
            vehicle.OwnerId,
            vehicle,
            new RoutePlanInputModel
            {
                VehicleId = vehicle.Id,
                Origin = "Setubal",
                Destination = "Faro",
                Waypoints = "Grandola",
                AverageSpeed = 100
            },
            BuildStations());

        var stops = smartSave.ReadSuggestedStops(route);

        Assert.True(route.RequiresIntermediateStops);
        Assert.True(route.SuggestedStopCount > 0);
        Assert.NotEmpty(stops);
        Assert.All(stops, stop => Assert.Equal(FuelKind.Electric, stop.FuelKind));
        Assert.True(route.TotalSuggestedReplenishmentAmount > 0);
        Assert.True(route.EstimatedArrivalLevelPercent >= route.MinimumArrivalLevelPercent);
        Assert.Contains("paragem", route.Recommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BuildRouteAsync_HigherSpeed_IncreasesConsumptionEstimate()
    {
        var smartSave = CreateSmartSaveService();
        var vehicle = new VehicleProfile
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Nickname = "Touring Diesel",
            Brand = "Peugeot",
            Model = "308 SW",
            Year = 2021,
            FuelKind = FuelKind.Diesel,
            TollClass = TollClass.Class1,
            AverageConsumption = 5.2,
            TankCapacity = 53,
            AverageRangeKm = 940,
            CurrentLevelPercent = 72
        };

        var slowRoute = await smartSave.BuildRouteAsync(
            vehicle.OwnerId,
            vehicle,
            new RoutePlanInputModel
            {
                VehicleId = vehicle.Id,
                Origin = "Setubal",
                Destination = "Faro",
                AverageSpeed = 80
            },
            BuildStations());

        var fastRoute = await smartSave.BuildRouteAsync(
            vehicle.OwnerId,
            vehicle,
            new RoutePlanInputModel
            {
                VehicleId = vehicle.Id,
                Origin = "Setubal",
                Destination = "Faro",
                AverageSpeed = 130
            },
            BuildStations());

        Assert.True(fastRoute.AdjustedConsumptionPer100 > slowRoute.AdjustedConsumptionPer100);
        Assert.True(fastRoute.EstimatedConsumptionAmount > slowRoute.EstimatedConsumptionAmount);
        Assert.True(fastRoute.ConsumptionSpeedFactor > slowRoute.ConsumptionSpeedFactor);
    }

    private static SmartSaveService CreateSmartSaveService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var httpClient = new HttpClient(new HttpClientHandler());
        var geocoding = new GeocodingService(httpClient, configuration);
        var directions = new GoogleDirectionsService(httpClient, configuration);
        return new SmartSaveService(geocoding, directions);
    }

    private static IReadOnlyList<FuelStation> BuildStations()
    {
        return
        [
            new FuelStation
            {
                ExternalId = "platform-prio-alcacer",
                Name = "Prio Alcacer Eco",
                Brand = "Prio",
                City = "Alcacer do Sal",
                District = "Setubal",
                Address = "EN5, Alcacer do Sal",
                Latitude = 38.3733,
                Longitude = -8.5144,
                FuelKind = FuelKind.Diesel,
                Price = 1.589,
                AvailableFuels = "Gasoleo simples, Gasolina 95",
                AvailableFuelKinds = "Diesel|Gasoline95",
                FuelPriceSummary = "Gasoleo 1.589 EUR · Gasolina 95 1.699 EUR",
                FuelPriceMapJson = "{\"Diesel\":1.589,\"Gasoline95\":1.699}"
            },
            new FuelStation
            {
                ExternalId = "platform-galp-palmela",
                Name = "Galp Palmela Norte",
                Brand = "Galp",
                City = "Palmela",
                District = "Setubal",
                Address = "A2 Palmela",
                Latitude = 38.5790,
                Longitude = -8.9010,
                FuelKind = FuelKind.Diesel,
                Price = 1.699,
                AvailableFuels = "Gasoleo simples, Gasolina 95, Gasolina 98, GPL",
                AvailableFuelKinds = "Diesel|Gasoline95|Gasoline98|GPL",
                FuelPriceSummary = "Gasoleo 1.699 EUR · Gasolina 95 1.789 EUR · Gasolina 98 1.929 EUR · GPL 0.899 EUR",
                FuelPriceMapJson = "{\"Diesel\":1.699,\"Gasoline95\":1.789,\"Gasoline98\":1.929,\"GPL\":0.899}"
            },
            new FuelStation
            {
                ExternalId = "platform-ev-grandola",
                Name = "Ionity Grandola",
                Brand = "Ionity",
                City = "Grandola",
                District = "Setubal",
                Address = "A2 Grandola",
                Latitude = 38.1840,
                Longitude = -8.5660,
                FuelKind = FuelKind.Electric,
                Price = 0.49,
                AvailableFuels = "Carregamento eletrico",
                AvailableFuelKinds = "Electric",
                FuelPriceSummary = "Eletricidade 0.490 EUR/kWh",
                FuelPriceMapJson = "{\"Electric\":0.49}",
                IsElectricCharging = true
            },
            new FuelStation
            {
                ExternalId = "platform-ev-faro",
                Name = "Galp Faro Charge",
                Brand = "Galp",
                City = "Faro",
                District = "Faro",
                Address = "Faro litoral",
                Latitude = 37.0194,
                Longitude = -7.9304,
                FuelKind = FuelKind.Electric,
                Price = 0.57,
                AvailableFuels = "Carregamento eletrico",
                AvailableFuelKinds = "Electric",
                FuelPriceSummary = "Eletricidade 0.570 EUR/kWh",
                FuelPriceMapJson = "{\"Electric\":0.57}",
                IsElectricCharging = true
            }
        ];
    }
}
