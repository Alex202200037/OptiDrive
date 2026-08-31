using System.Diagnostics;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;
using Xunit;

namespace OptiDrive.Web.Tests;

public sealed class StressTests
{
    [Fact]
    public void LocalState_With500UsersAnd500Vehicles_RemainsResponsive()
    {
        var appState = new AppStateService();
        var createdUsers = new List<UserAccount>(capacity: 500);
        var stopwatch = Stopwatch.StartNew();

        for (var index = 0; index < 500; index++)
        {
            var user = appState.Register(
                $"Stress Driver {index:000}",
                $"stress.driver.{index:000}@optidrive.test",
                "stress123");

            appState.AddVehicle(user.Id, new VehicleInputModel
            {
                Nickname = $"Stress Vehicle {index:000}",
                RegistrationPlate = $"ST-{index:000}",
                Brand = index % 3 == 0 ? "Toyota" : index % 3 == 1 ? "Peugeot" : "Tesla",
                Model = index % 3 == 0 ? "Yaris Hybrid" : index % 3 == 1 ? "308 SW" : "Model 3",
                Year = 2020 + index % 5,
                FuelKind = index % 5 == 0 ? FuelKind.Electric : index % 2 == 0 ? FuelKind.Diesel : FuelKind.Gasoline95,
                TollClass = TollClass.Class1,
                AverageConsumption = index % 5 == 0 ? 16.2 : index % 2 == 0 ? 5.4 : 6.1,
                TankCapacity = index % 5 == 0 ? 0 : 50,
                BatteryCapacity = index % 5 == 0 ? 60 : null,
                AverageRangeKm = index % 5 == 0 ? 410 : 820,
                CurrentLevelPercent = 35 + index % 60,
                OdometerKm = 10000 + index * 37,
                TireHealthPercent = 70 + index % 25,
                Notes = "Registo gerado para teste de stress local."
            });

            createdUsers.Add(user);
        }

        var creationElapsed = stopwatch.Elapsed;
        stopwatch.Restart();

        foreach (var user in createdUsers)
        {
            var dashboard = appState.BuildDashboard(user.Id);
            Assert.Single(dashboard.Vehicles);
            Assert.NotEmpty(dashboard.SocialDirectory);
            Assert.NotEmpty(dashboard.Stations);
        }

        var dashboardElapsed = stopwatch.Elapsed;

        Assert.True(appState.UserCount() >= 503);
        Assert.True(appState.VehicleCount() >= 502);
        Assert.True(creationElapsed.TotalSeconds < 10, $"Criação demorou {creationElapsed.TotalSeconds:F2}s");
        Assert.True(dashboardElapsed.TotalSeconds < 10, $"Dashboards demoraram {dashboardElapsed.TotalSeconds:F2}s");
    }
}
