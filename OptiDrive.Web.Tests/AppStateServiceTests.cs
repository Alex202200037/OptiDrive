using OptiDrive.Web.Models;
using OptiDrive.Web.Services;
using Xunit;

namespace OptiDrive.Web.Tests;

public sealed class AppStateServiceTests
{
    [Fact]
    public void AddVehicle_ForNewUser_CreatesDefaultVehicleAndHistoryEntry()
    {
        var appState = new AppStateService();
        var user = appState.Register("QA Driver", "qa-driver@optidrive.pt", "secret123");

        var vehicleId = appState.AddVehicle(user.Id, new VehicleInputModel
        {
            Nickname = "Meu EV",
            Brand = "Tesla",
            Model = "Model Y",
            Year = 2024,
            FuelKind = FuelKind.Electric,
            TollClass = TollClass.Class1,
            AverageConsumption = 16.4,
            TankCapacity = 0,
            BatteryCapacity = 75,
            AverageRangeKm = 470,
            CurrentLevelPercent = 81,
            OdometerKm = 1450,
            TireHealthPercent = 94
        });

        var dashboard = appState.BuildDashboard(user.Id);
        var vehicle = Assert.Single(dashboard.Vehicles);

        Assert.Equal(vehicleId, vehicle.Id);
        Assert.True(vehicle.IsDefault);
        Assert.Equal("Tesla", vehicle.Brand);
        Assert.Equal("Model Y", vehicle.Model);

        var createdActivity = Assert.Single(dashboard.VehicleActivities);
        Assert.Equal(VehicleActivityType.Created, createdActivity.Type);
        Assert.Contains("Veículo adicionado", createdActivity.Title);
        Assert.Equal(81, createdActivity.LevelPercentAfter);
    }

    [Fact]
    public void RefuelVehicle_UpdatesLevelCostAndGarageHistory()
    {
        var appState = new AppStateService();
        var user = appState.Register("Fuel QA", "fuel-qa@optidrive.pt", "secret123");
        var vehicleId = appState.AddVehicle(user.Id, new VehicleInputModel
        {
            Nickname = "Carro de teste",
            Brand = "Peugeot",
            Model = "308",
            Year = 2022,
            FuelKind = FuelKind.Diesel,
            TollClass = TollClass.Class1,
            AverageConsumption = 5.2,
            TankCapacity = 50,
            AverageRangeKm = 850,
            CurrentLevelPercent = 40,
            OdometerKm = 32000,
            TireHealthPercent = 90
        });

        appState.RefuelVehicle(user.Id, new RefuelVehicleInputModel
        {
            Id = vehicleId,
            Amount = 10,
            UnitPrice = 1.65,
            StationName = "Posto de validação"
        });

        var afterRefuel = appState.BuildDashboard(user.Id);
        var vehicle = Assert.Single(afterRefuel.Vehicles);
        var refuel = afterRefuel.VehicleActivities.First(activity => activity.Type == VehicleActivityType.Refuel);

        Assert.Equal(60, vehicle.CurrentLevelPercent);
        Assert.Equal(10, refuel.Amount);
        Assert.Equal(16.50, refuel.CostEstimate);
        Assert.Equal("L", refuel.Unit);
        Assert.Contains("Posto de validação", refuel.Details);

        appState.RefuelVehicle(user.Id, new RefuelVehicleInputModel
        {
            Id = vehicleId,
            FillToFull = true
        });

        var afterFill = appState.BuildDashboard(user.Id);
        Assert.Equal(100, Assert.Single(afterFill.Vehicles).CurrentLevelPercent);
        Assert.Contains(afterFill.VehicleActivities, activity =>
            activity.Type == VehicleActivityType.Refuel
            && activity.LevelPercentAfter == 100
            && activity.Title.Contains("cheio", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyRouteToVehicle_UpdatesVehicleStateAndMarksRouteAsApplied()
    {
        var appState = new AppStateService();
        var user = appState.ValidateUser("alexandre@optidrive.pt", "opti2026");

        Assert.NotNull(user);
        var before = appState.BuildDashboard(user!.Id);
        var route = Assert.Single(before.Routes);
        var vehicle = Assert.Single(before.Vehicles.Where(item => item.Id == route.VehicleId));
        var startingOdometer = vehicle.OdometerKm;

        var result = appState.ApplyRouteToVehicle(user.Id, route.Id);

        Assert.True(result.Success);

        var after = appState.BuildDashboard(user.Id);
        var updatedRoute = Assert.Single(after.Routes);
        var updatedVehicle = Assert.Single(after.Vehicles.Where(item => item.Id == route.VehicleId));
        var tripActivity = after.VehicleActivities.FirstOrDefault(activity => activity.RelatedRouteId == route.Id);

        Assert.True(updatedRoute.IsAppliedToVehicle);
        Assert.NotNull(updatedRoute.AppliedAtUtc);
        Assert.Equal(startingOdometer + route.DistanceKm, updatedVehicle.OdometerKm, 1);
        Assert.Equal(route.EstimatedArrivalLevelPercent, updatedVehicle.CurrentLevelPercent);
        Assert.NotNull(tripActivity);
        Assert.Equal(VehicleActivityType.TripApplied, tripActivity!.Type);
        Assert.Contains("Viagem", tripActivity.Title);
    }

    [Fact]
    public void SendDirectMessage_AddsConversationToSocialDashboard()
    {
        var appState = new AppStateService();
        var user = appState.ValidateUser("alexandre@optidrive.pt", "opti2026");

        Assert.NotNull(user);
        appState.SendDirectMessage(user!.Id, new DirectMessageInputModel
        {
            RecipientEmail = "marta.costa@optidrive.pt",
            Body = "Seguimos juntos pela rota Smart Save?"
        });

        var dashboard = appState.BuildDashboard(user.Id);
        var latest = dashboard.DirectMessages.First();

        Assert.Equal(user.Id, latest.SenderUserId);
        Assert.Equal("marta.costa@optidrive.pt", latest.RecipientEmail);
        Assert.Contains("Smart Save", latest.Body);
    }

    [Fact]
    public void CollaborativeTrip_CanBeCreatedJoinedAndStarted()
    {
        var appState = new AppStateService();
        var user = appState.ValidateUser("alexandre@optidrive.pt", "opti2026");

        Assert.NotNull(user);
        var dashboard = appState.BuildDashboard(user!.Id);
        var route = dashboard.Routes.First();
        var tripId = appState.CreateCollaborativeTrip(user.Id, new CollaborativeTripInputModel
        {
            Name = "Viagem social",
            RouteId = route.Id,
            VehicleId = route.VehicleId,
            MeetingPoint = "Palmela",
            MemberEmails = "marta.costa@optidrive.pt",
            EstimatedSplitAmount = 12.5m,
            Notes = "Viagem criada pela suite de qualidade."
        });

        appState.JoinCollaborativeTrip(user.Id, tripId);
        appState.StartCollaborativeTrip(user.Id, tripId);

        var after = appState.BuildDashboard(user.Id);
        var trip = Assert.Single(after.CollaborativeTrips.Where(item => item.Id == tripId));

        Assert.Equal(CollaborativeTripStatus.Active, trip.Status);
        Assert.Contains("alexandre@optidrive.pt", trip.Members);
        Assert.Contains("marta.costa@optidrive.pt", trip.Members);
        Assert.Contains(after.DirectMessages, message => message.RelatedTripId == tripId && message.Body.Contains("iniciada"));
    }

    [Fact]
    public void Register_StoresHashedPasswordAndValidatesLogin()
    {
        var appState = new AppStateService();

        var user = appState.Register("Secure Driver", "secure-driver@optidrive.pt", "secret123");
        var login = appState.ValidateUser("secure-driver@optidrive.pt", "secret123");

        Assert.NotNull(login);
        Assert.Equal(user.Id, login!.Id);
        Assert.NotEqual("secret123", user.PasswordHash);
        Assert.StartsWith("PBKDF2-SHA256$", user.PasswordHash);
        Assert.Empty(user.Password);
    }

    [Fact]
    public void Authenticator_CanBeEnabledAndVerifiedWithTotp()
    {
        var authenticator = new AuthenticatorService();
        var appState = new AppStateService(authenticatorService: authenticator);
        var user = appState.Register("MFA Driver", "mfa-driver@optidrive.pt", "secret123");
        var setup = appState.GetAuthenticatorSetup(user.Id);
        var rawSecret = setup.SharedKey.Replace(" ", string.Empty, StringComparison.Ordinal);
        var code = authenticator.GenerateCode(rawSecret);

        var enabled = appState.EnableAuthenticator(user.Id, code, out var recoveryCodes);
        var verified = appState.VerifySecondFactor(user.Id, authenticator.GenerateCode(rawSecret));

        Assert.True(enabled);
        Assert.True(verified);
        Assert.NotEmpty(setup.QrCodeDataUri);
        Assert.Equal(8, recoveryCodes.Count);
        Assert.True(appState.GetUser(user.Id)!.TwoFactorEnabled);
    }

    [Fact]
    public void AdminActions_ManageUserSecurityWithoutAllowingRegularUsers()
    {
        var appState = new AppStateService();
        var admin = appState.ValidateUser("admin@optidrive.pt", "admin123");
        var target = appState.Register("Managed Driver", "managed-driver@optidrive.pt", "secret123");
        var regularUser = appState.ValidateUser("alexandre@optidrive.pt", "opti2026");

        Assert.NotNull(admin);
        Assert.NotNull(regularUser);

        var blocked = appState.TrySetUserLock(admin!.Id, target.Id, true, out var lockMessage);
        var regularAttempt = appState.TrySetUserRole(regularUser!.Id, target.Id, UserRole.Admin, out var regularMessage);
        var unlocked = appState.TrySetUserLock(admin.Id, target.Id, false, out var unlockMessage);
        var promoted = appState.TrySetUserRole(admin.Id, target.Id, UserRole.Admin, out var roleMessage);
        var resetMfa = appState.TryResetUserMfa(admin.Id, target.Id, out var mfaMessage);

        Assert.True(blocked);
        Assert.Contains("bloqueado", lockMessage);
        Assert.False(regularAttempt);
        Assert.Contains("Apenas administradores", regularMessage);
        Assert.True(unlocked);
        Assert.Contains("desbloqueado", unlockMessage);
        Assert.True(promoted);
        Assert.Contains("administrador", roleMessage);
        Assert.True(resetMfa);
        Assert.Contains("MFA", mfaMessage);
        Assert.True(appState.GetUser(target.Id)!.Role == UserRole.Admin);
        Assert.False(appState.GetUser(target.Id)!.TwoFactorEnabled);
    }
}
