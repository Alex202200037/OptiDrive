using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OptiDrive.Web.Data;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class AppStateService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly object _syncRoot = new();
    private readonly List<UserAccount> _users;
    private readonly List<VehicleProfile> _vehicles;
    private readonly List<FuelStation> _stations;
    private readonly List<PlannedRoute> _routes;
    private readonly List<SocialProfile> _socialProfiles;
    private readonly List<TrustedContact> _contacts;
    private readonly List<SharedVehicle> _shares;
    private readonly List<TravelGroup> _groups;
    private readonly List<DirectMessage> _messages;
    private readonly List<CollaborativeTrip> _collaborativeTrips;
    private readonly List<PriceReport> _reports;
    private readonly List<VehicleActivity> _vehicleActivities;
    private readonly List<SocialConnectionRequest> _connectionRequests;
    private readonly IDbContextFactory<OptiDriveDbContext>? _dbFactory;
    private readonly AuthenticatorService _authenticatorService;
    private readonly Dictionary<string, ApiSyncStatus> _syncStatuses = new(StringComparer.OrdinalIgnoreCase);

    public AppStateService(IDbContextFactory<OptiDriveDbContext>? dbFactory = null, AuthenticatorService? authenticatorService = null)
    {
        _dbFactory = dbFactory;
        _authenticatorService = authenticatorService ?? new AuthenticatorService();
        var primaryUser = new UserAccount
        {
            Name = "Alexandre Miguel",
            Email = "alexandre@optidrive.pt",
            PasswordHash = PasswordSecurityService.HashPassword("opti2026"),
            EmailConfirmed = true,
            Role = UserRole.User
        };

        var adminUser = new UserAccount
        {
            Name = "Alexandre Admin",
            Email = "admin@optidrive.pt",
            PasswordHash = PasswordSecurityService.HashPassword("admin123"),
            EmailConfirmed = true,
            Role = UserRole.Admin
        };

        _users =
        [
            primaryUser,
            adminUser,
            new UserAccount
            {
                Name = "Marta Costa",
                Email = "marta.costa@optidrive.pt",
                PasswordHash = PasswordSecurityService.HashPassword("marta123"),
                EmailConfirmed = true
            }
        ];
        _socialProfiles =
        [
            new SocialProfile
            {
                UserId = primaryUser.Id,
                DisplayName = "Alexandre Miguel",
                Bio = "Condutor focado em poupança, viagens longas e partilha de rotas com amigos.",
                HomeCity = "Setubal",
                DrivingStyle = "Económico",
                PreferredFuelBrands = "Galp, Prio, Ionity",
                IsOpenToCarpool = true,
                ShareLiveTripStatus = true
            },
            new SocialProfile
            {
                UserId = adminUser.Id,
                DisplayName = "Alexandre Admin",
                Bio = "Gestão da plataforma OptiDrive, backoffice e validação de integrações.",
                HomeCity = "Setubal",
                DrivingStyle = "Equilibrado",
                PreferredFuelBrands = "Galp, Repsol, BP",
                IsOpenToCarpool = false,
                ShareLiveTripStatus = false
            }
        ];

        var dieselVehicle = new VehicleProfile
        {
            OwnerId = primaryUser.Id,
            Nickname = "Carrinha de Viagem",
            RegistrationPlate = "34-AB-12",
            Brand = "Peugeot",
            Model = "308 SW",
            Year = 2021,
            FuelKind = FuelKind.Diesel,
            TollClass = TollClass.Class1,
            AverageConsumption = 5.2,
            TankCapacity = 53,
            AverageRangeKm = 940,
            CurrentLevelPercent = 68,
            OdometerKm = 64210,
            NextServiceKm = 75000,
            IsDefault = true,
            LastMaintenance = new DateOnly(2026, 3, 20),
            TireHealthPercent = 74,
            InspectionDueDate = new DateOnly(2026, 11, 18),
            InsuranceDueDate = new DateOnly(2026, 9, 30),
            TaxDueDate = new DateOnly(2026, 7, 15),
            Notes = "Veiculo ideal para viagens longas e consumos estaveis."
        };

        var electricVehicle = new VehicleProfile
        {
            OwnerId = primaryUser.Id,
            Nickname = "Eletrico Urbano",
            RegistrationPlate = "91-ZE-44",
            Brand = "Tesla",
            Model = "Model 3",
            Year = 2024,
            FuelKind = FuelKind.Electric,
            TollClass = TollClass.Class1,
            AverageConsumption = 15.8,
            TankCapacity = 0,
            BatteryCapacity = 60,
            AverageRangeKm = 420,
            CurrentLevelPercent = 82,
            OdometerKm = 18240,
            NextServiceKm = 30000,
            TireHealthPercent = 88,
            InspectionDueDate = new DateOnly(2028, 2, 10),
            InsuranceDueDate = new DateOnly(2026, 8, 24),
            TaxDueDate = new DateOnly(2026, 6, 30),
            Notes = "Usado para cidade e viagens medias com carregamento rapido."
        };

        _vehicles =
        [
            dieselVehicle,
            electricVehicle
        ];

        _stations =
        [
            new() { Name = "Prio Alcacer Eco", Brand = "Prio", City = "Alcacer do Sal", District = "Setubal", Latitude = 38.3733, Longitude = -8.5144, FuelKind = FuelKind.Diesel, Price = 1.589, IsLowCost = true, IsOpen24h = true, Services = "Loja,Ar/Agua", AvailableFuels = "Gasoleo simples, Gasolina 95", AvailableFuelKinds = "Diesel|Gasoline95", FuelPriceSummary = "Gasoleo 1.589 EUR · Gasolina 95 1.699 EUR", FuelPriceMapJson = "{\"Diesel\":1.589,\"Gasoline95\":1.699}", Source = "Platform" },
            new() { Name = "Galp Palmela Norte", Brand = "Galp", City = "Palmela", District = "Setubal", Latitude = 38.5790, Longitude = -8.9010, FuelKind = FuelKind.Diesel, Price = 1.699, IsOpen24h = true, Services = "Cafe,Loja,WC", AvailableFuels = "Gasoleo simples, Gasolina 95, Gasolina 98, GPL", AvailableFuelKinds = "Diesel|Gasoline95|Gasoline98|GPL", FuelPriceSummary = "Gasoleo 1.699 EUR · Gasolina 95 1.789 EUR · Gasolina 98 1.929 EUR · GPL 0.899 EUR", FuelPriceMapJson = "{\"Diesel\":1.699,\"Gasoline95\":1.789,\"Gasoline98\":1.929,\"GPL\":0.899}", Source = "Platform" },
            new() { Name = "Auchan Lisboa Sul", Brand = "Auchan", City = "Almada", District = "Lisboa", Latitude = 38.6760, Longitude = -9.1720, FuelKind = FuelKind.Gasoline95, Price = 1.634, IsLowCost = true, IsOpen24h = true, Services = "Cafe,Lavagem,Loja", AvailableFuels = "Gasolina 95, Gasoleo simples", AvailableFuelKinds = "Gasoline95|Diesel", FuelPriceSummary = "Gasolina 95 1.634 EUR · Gasoleo 1.574 EUR", FuelPriceMapJson = "{\"Gasoline95\":1.634,\"Diesel\":1.574}", Source = "Platform" },
            new() { Name = "BP Evora Gateway", Brand = "BP", City = "Evora", District = "Evora", Latitude = 38.5660, Longitude = -7.9060, FuelKind = FuelKind.Gasoline95, Price = 1.764, Services = "Cafe,Lavagem", AvailableFuels = "Gasolina 95, Gasolina 98, Gasoleo simples", AvailableFuelKinds = "Gasoline95|Gasoline98|Diesel", FuelPriceSummary = "Gasolina 95 1.764 EUR · Gasolina 98 1.919 EUR · Gasoleo 1.699 EUR", FuelPriceMapJson = "{\"Gasoline95\":1.764,\"Gasoline98\":1.919,\"Diesel\":1.699}", Source = "Platform" },
            new() { Name = "Ionity Grandola", Brand = "Ionity", City = "Grandola", District = "Setubal", Latitude = 38.1840, Longitude = -8.5660, FuelKind = FuelKind.Electric, Price = 0.49, IsOpen24h = true, Services = "FastCharge,WC", AvailableFuels = "Carregamento eletrico", AvailableFuelKinds = "Electric", FuelPriceSummary = "Eletricidade 0.490 EUR/kWh", FuelPriceMapJson = "{\"Electric\":0.49}", IsElectricCharging = true, Source = "Platform" },
            new() { Name = "Continente Charge Setubal", Brand = "Continente", City = "Setubal", District = "Setubal", Latitude = 38.5320, Longitude = -8.8890, FuelKind = FuelKind.Electric, Price = 0.42, IsLowCost = true, IsOpen24h = true, Services = "Loja,Cafe,FastCharge", AvailableFuels = "Carregamento eletrico", AvailableFuelKinds = "Electric", FuelPriceSummary = "Eletricidade 0.420 EUR/kWh", FuelPriceMapJson = "{\"Electric\":0.42}", IsElectricCharging = true, Source = "Platform" },
            new() { Name = "Galp Faro Litoral", Brand = "Galp", City = "Faro", District = "Faro", Latitude = 37.0190, Longitude = -7.9320, FuelKind = FuelKind.Gasoline95, Price = 1.719, IsOpen24h = true, Services = "Cafe,Loja,Ar/Agua", AvailableFuels = "Gasolina 95, Gasoleo simples", AvailableFuelKinds = "Gasoline95|Diesel", FuelPriceSummary = "Gasolina 95 1.719 EUR · Gasoleo 1.659 EUR", FuelPriceMapJson = "{\"Gasoline95\":1.719,\"Diesel\":1.659}", Source = "Platform" }
        ];

        _routes =
        [
            new()
            {
                OwnerId = primaryUser.Id,
                VehicleId = dieselVehicle.Id,
                Origin = "Setubal",
                Destination = "Faro",
                Waypoints = "Alcacer do Sal,Grandola",
                AvoidTolls = false,
                DistanceKm = 241,
                EtaMinutes = 154,
                FastestCost = 37.24,
                CheapestCost = 35.41,
                TollCost = 15.50,
                CarbonKg = 28.3,
                EstimatedConsumptionAmount = 12.5,
                AverageSpeedKmh = 94,
                AdjustedConsumptionPer100 = 5.2,
                ConsumptionSpeedFactor = 1,
                MinimumArrivalLevelPercent = 8,
                TotalSuggestedReplenishmentAmount = 0,
                EstimatedArrivalLevelPercent = 44,
                EstimatedDirectArrivalLevelPercent = 44,
                SuggestedStopCount = 0,
                RequiresIntermediateStops = false,
                SavingsScore = 8,
                Recommendation = "Parar em Prio Alcacer Eco para beneficiar do preco low-cost.",
                CriticalAlert = "Autonomia suficiente para executar a viagem com margem.",
                RecommendedStationName = "Prio Alcacer Eco",
                RecommendedChargeAmount = 24
            }
        ];

        _contacts = [new TrustedContact { OwnerId = primaryUser.Id, ContactName = "Marta Costa", ContactEmail = "marta.costa@optidrive.pt" }];
        _connectionRequests = [];
        _shares = [new SharedVehicle { OwnerId = primaryUser.Id, VehicleId = dieselVehicle.Id, TargetEmail = "marta.costa@optidrive.pt", CanEdit = false }];
        _groups = [new TravelGroup { OwnerId = primaryUser.Id, Name = "Roadtrip Algarve", Description = "Caravana de fim de semana", SplitAmount = 17.70m, Members = ["alexandre@optidrive.pt", "marta.costa@optidrive.pt"] }];
        _messages =
        [
            new DirectMessage
            {
                OwnerId = primaryUser.Id,
                SenderUserId = _users[2].Id,
                SenderName = "Marta Costa",
                RecipientEmail = primaryUser.Email,
                RecipientName = primaryUser.Name,
                ThreadKey = BuildThreadKey(primaryUser.Email, "marta.costa@optidrive.pt"),
                Body = "Confirmas se vamos pela A2 ou preferes evitar portagens?",
                SentAtUtc = DateTime.UtcNow.AddHours(-7)
            },
            new DirectMessage
            {
                OwnerId = primaryUser.Id,
                SenderUserId = primaryUser.Id,
                SenderName = primaryUser.Name,
                RecipientEmail = "marta.costa@optidrive.pt",
                RecipientName = "Marta Costa",
                ThreadKey = BuildThreadKey(primaryUser.Email, "marta.costa@optidrive.pt"),
                Body = "Vou validar no Smart Save e partilho a rota com paragens.",
                SentAtUtc = DateTime.UtcNow.AddHours(-6)
            }
        ];
        _collaborativeTrips =
        [
            new CollaborativeTrip
            {
                OwnerId = primaryUser.Id,
                RouteId = _routes[0].Id,
                VehicleId = dieselVehicle.Id,
                Name = "Algarve em grupo",
                Origin = "Setubal",
                Destination = "Faro",
                MeetingPoint = "Estação de Serviço de Palmela",
                StartsAtUtc = DateTime.UtcNow.AddDays(3).Date.AddHours(9),
                Status = CollaborativeTripStatus.Scheduled,
                Members = ["alexandre@optidrive.pt", "marta.costa@optidrive.pt"],
                EstimatedSplitAmount = 17.70m,
                Notes = "Confirmar boleia, combustível dividido e paragem em Alcacer."
            }
        ];
        _reports = [new PriceReport { StationId = _stations[1].Id, UserId = primaryUser.Id, ReporterName = primaryUser.Name, ReportedPrice = 1.689m, Note = "No painel local estava ligeiramente mais baixo." }];
        _vehicleActivities =
        [
            new VehicleActivity
            {
                OwnerId = primaryUser.Id,
                VehicleId = dieselVehicle.Id,
                Type = VehicleActivityType.Refuel,
                Title = "Abastecimento registado",
                Details = "Abastecidos 31.2 L antes da ultima viagem longa.",
                Amount = 31.2,
                Unit = "L",
                UnitPrice = 1.659,
                CostEstimate = 51.76,
                OdometerKm = 63980,
                LevelPercentAfter = 96,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new VehicleActivity
            {
                OwnerId = primaryUser.Id,
                VehicleId = dieselVehicle.Id,
                Type = VehicleActivityType.TripApplied,
                Title = "Viagem Setubal -> Faro assumida",
                Details = "Viagem principal do fim de semana com custo otimizado.",
                RelatedRouteId = _routes[0].Id,
                Amount = _routes[0].DistanceKm,
                Unit = "km",
                CostEstimate = _routes[0].CheapestCost,
                OdometerKm = 64210,
                LevelPercentAfter = 68,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new VehicleActivity
            {
                OwnerId = primaryUser.Id,
                VehicleId = electricVehicle.Id,
                Type = VehicleActivityType.Charge,
                Title = "Carga rápida concluída",
                Details = "Carregamento em Continente Charge Setubal.",
                Amount = 24.6,
                Unit = "kWh",
                UnitPrice = 0.42,
                CostEstimate = 10.33,
                OdometerKm = 18120,
                LevelPercentAfter = 82,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new VehicleActivity
            {
                OwnerId = primaryUser.Id,
                VehicleId = electricVehicle.Id,
                Type = VehicleActivityType.Updated,
                Title = "Dados do veículo afinados",
                Details = "Consumo, autonomia e revisão ajustados para refletir uso real.",
                OdometerKm = 18240,
                LevelPercentAfter = 82,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        ];
        _syncStatuses["VehicleCatalog"] = new ApiSyncStatus { SourceName = "vPIC", Message = "Ainda sem sincronizacao.", IsSuccess = false };
        _syncStatuses["FuelStations"] = new ApiSyncStatus { SourceName = "DGEG", Message = "A aguardar primeira sincronizacao externa.", IsSuccess = false, ItemCount = _stations.Count };
        LoadPersistedSnapshotOrSaveBaseline();
        EnsureLiveSampleData();
    }

    private void LoadPersistedSnapshotOrSaveBaseline()
    {
        if (_dbFactory is null)
        {
            return;
        }

        using var db = _dbFactory.CreateDbContext();
        db.Database.EnsureCreated();
        if (!db.Users.Any())
        {
            PersistSnapshot();
            return;
        }

        _users.Clear();
        _users.AddRange(db.Users.AsNoTracking().ToList());
        _socialProfiles.Clear();
        _socialProfiles.AddRange(db.SocialProfiles.AsNoTracking().ToList());
        _vehicles.Clear();
        _vehicles.AddRange(db.Vehicles.AsNoTracking().ToList());
        _stations.Clear();
        _stations.AddRange(db.Stations.AsNoTracking().ToList());
        _routes.Clear();
        _routes.AddRange(db.Routes.AsNoTracking().ToList());
        ApplyRouteCompatibilityDefaults();
        _contacts.Clear();
        _contacts.AddRange(db.Contacts.AsNoTracking().ToList());
        _connectionRequests.Clear();
        _connectionRequests.AddRange(db.SocialConnectionRequests.AsNoTracking().ToList());
        _shares.Clear();
        _shares.AddRange(db.SharedVehicles.AsNoTracking().ToList());
        _groups.Clear();
        _groups.AddRange(db.TravelGroups.AsNoTracking().ToList());
        _messages.Clear();
        _messages.AddRange(db.DirectMessages.AsNoTracking().ToList());
        _collaborativeTrips.Clear();
        _collaborativeTrips.AddRange(db.CollaborativeTrips.AsNoTracking().ToList());
        _reports.Clear();
        _reports.AddRange(db.PriceReports.AsNoTracking().ToList());
        _vehicleActivities.Clear();
        _vehicleActivities.AddRange(db.VehicleActivities.AsNoTracking().ToList());
        _syncStatuses.Clear();
        foreach (var status in db.SyncStatuses.AsNoTracking())
        {
            _syncStatuses[status.SourceName] = status;
        }
    }

    private void EnsureLiveSampleData()
    {
        var changed = false;
        var now = DateTime.UtcNow;
        RemoveInternalValidationUsers(ref changed);

        var primary = EnsurePresentationUser(
            "Alexandre Miguel",
            "alexandre@optidrive.pt",
            "opti2026",
            "Local",
            "Local",
            true,
            now.AddMinutes(-18),
            "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=240&q=80",
            ref changed);
        var marta = EnsurePresentationUser(
            "Marta Costa",
            "marta.costa@optidrive.pt",
            "marta123",
            "Google",
            "Local, Google",
            false,
            now.AddMinutes(-42),
            "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=240&q=80",
            ref changed);
        var joao = EnsurePresentationUser(
            "Joao Pereira",
            "joao.pereira@optidrive.pt",
            "joao123",
            "Google",
            "Google",
            false,
            now.AddHours(-2),
            "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=240&q=80",
            ref changed);
        var ines = EnsurePresentationUser(
            "Ines Martins",
            "ines.martins@optidrive.pt",
            "ines123",
            "Microsoft",
            "Microsoft",
            false,
            now.AddMinutes(-9),
            "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=240&q=80",
            ref changed);
        var rui = EnsurePresentationUser(
            "Rui Almeida",
            "rui.almeida@optidrive.pt",
            "rui123",
            "Local",
            "Local",
            false,
            now.AddHours(-19),
            "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=240&q=80",
            ref changed);
        var sofia = EnsurePresentationUser(
            "Sofia Rocha",
            "sofia.rocha@optidrive.pt",
            "sofia123",
            "Google",
            "Google",
            false,
            now.AddHours(-5),
            "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?auto=format&fit=crop&w=240&q=80",
            ref changed);

        EnsurePresentationProfile(primary, "Setubal", "Económico", "Galp, Prio, Ionity", "Condutor focado em poupança, viagens longas e partilha de rotas com amigos.", true, true, ref changed);
        EnsurePresentationProfile(marta, "Almada", "Equilibrado", "Prio, Galp, Continente", "Gosta de roadtrips curtas, boleias combinadas e comparar custos antes de sair.", true, true, ref changed);
        EnsurePresentationProfile(joao, "Lisboa", "Económico", "Repsol, Prio, BP", "Faz muitos quilómetros em autoestrada e usa o OptiDrive para dividir custos com colegas.", true, true, ref changed);
        EnsurePresentationProfile(ines, "Palmela", "Eco EV", "Ionity, Continente, Galp", "Perfil EV: planeia carregamentos rápidos e prefere chegar sempre com margem de bateria.", true, true, ref changed);
        EnsurePresentationProfile(rui, "Evora", "Equilibrado", "Galp, Auchan, Prio", "Usa GPL no dia-a-dia e gosta de encontrar postos low-cost fora dos centros urbanos.", true, false, ref changed);
        EnsurePresentationProfile(sofia, "Porto", "Rápido", "BP, Repsol, Galp", "Viaja frequentemente entre Porto, Lisboa e Algarve e partilha boleias com amigos.", false, true, ref changed);

        var primaryDiesel = EnsurePresentationVehicle(primary, "34-AB-12", "Carrinha de Viagem", "Peugeot", "308 SW 1.5 BlueHDi", 2021, FuelKind.Diesel, TollClass.Class1, 5.2, 53, null, 940, 68, 64210, 75000, true, new DateOnly(2026, 3, 20), 74, new DateOnly(2026, 11, 18), new DateOnly(2026, 9, 30), new DateOnly(2026, 7, 15), "Consumo real medido em viagens Setubal-Algarve: 5.1 a 5.4 L/100 km.", ref changed);
        var primaryEv = EnsurePresentationVehicle(primary, "91-ZE-44", "Eletrico Urbano", "Tesla", "Model 3 RWD", 2024, FuelKind.Electric, TollClass.Class1, 15.8, 0, 60, 420, 82, 18240, 30000, false, new DateOnly(2026, 4, 10), 88, new DateOnly(2028, 2, 10), new DateOnly(2026, 8, 24), new DateOnly(2026, 6, 30), "Consumo real entre 14.5 e 17.5 kWh/100 km conforme velocidade e temperatura.", ref changed);
        var martaYaris = EnsurePresentationVehicle(marta, "78-MT-22", "Yaris Hibrido", "Toyota", "Yaris Hybrid 1.5", 2022, FuelKind.Gasoline95, TollClass.Class1, 4.3, 36, null, 770, 54, 41890, 55000, true, new DateOnly(2026, 2, 12), 79, new DateOnly(2027, 1, 18), new DateOnly(2026, 10, 3), new DateOnly(2026, 9, 1), "Media real em cidade/IC: 4.1 a 4.6 L/100 km.", ref changed);
        var martaZoe = EnsurePresentationVehicle(marta, "24-EV-91", "Zoe da Marta", "Renault", "Zoe R135 ZE50", 2021, FuelKind.Electric, TollClass.Class1, 16.5, 0, 52, 315, 71, 52340, 65000, false, new DateOnly(2026, 1, 7), 69, new DateOnly(2027, 5, 9), new DateOnly(2026, 12, 14), new DateOnly(2026, 7, 31), "Autonomia real de 270 a 330 km; bom para viagens curtas com carga intermédia.", ref changed);
        var joaoGolf = EnsurePresentationVehicle(joao, "11-JP-20", "Golf TDI", "Volkswagen", "Golf 2.0 TDI", 2020, FuelKind.Diesel, TollClass.Class1, 5.0, 50, null, 980, 63, 87210, 90000, true, new DateOnly(2025, 12, 18), 62, new DateOnly(2026, 8, 11), new DateOnly(2026, 11, 25), new DateOnly(2026, 8, 31), "Media real em autoestrada a 110-120 km/h: 4.8 a 5.3 L/100 km.", ref changed);
        var inesKauai = EnsurePresentationVehicle(ines, "64-IN-22", "Kauai EV", "Hyundai", "Kauai Electric 64 kWh", 2022, FuelKind.Electric, TollClass.Class1, 15.2, 0, 64, 450, 38, 36680, 50000, true, new DateOnly(2026, 5, 3), 83, new DateOnly(2027, 9, 19), new DateOnly(2026, 8, 7), new DateOnly(2026, 6, 30), "Consumo real excelente em nacionais; em autoestrada sobe para 18-20 kWh/100 km.", ref changed);
        var ruiSandero = EnsurePresentationVehicle(rui, "55-RG-23", "Sandero GPL", "Dacia", "Sandero Stepway Bi-Fuel", 2023, FuelKind.GPL, TollClass.Class1, 7.4, 40, null, 540, 86, 27420, 42000, true, new DateOnly(2026, 4, 2), 91, new DateOnly(2027, 3, 21), new DateOnly(2026, 9, 2), new DateOnly(2026, 7, 12), "Media real em GPL: 7.2 a 7.8 L/100 km; custo por km muito baixo.", ref changed);
        var sofiaBmw = EnsurePresentationVehicle(sofia, "19-SR-19", "Serie 3 Touring", "BMW", "320d Touring", 2019, FuelKind.Diesel, TollClass.Class1, 5.4, 59, null, 1050, 47, 119800, 123000, true, new DateOnly(2025, 11, 28), 58, new DateOnly(2026, 7, 20), new DateOnly(2026, 12, 5), new DateOnly(2026, 7, 31), "Media real em viagens longas: 5.2 a 5.8 L/100 km.", ref changed);

        EnsurePresentationActivity(marta.Id, martaYaris, VehicleActivityType.Refuel, "Abastecimento Galp Almada", "28.4 L de gasolina 95 antes de viagem para Setubal.", 28.4, "L", 49.42, 1.740, 41760, 91, now.AddDays(-8), null, ref changed);
        EnsurePresentationActivity(marta.Id, martaYaris, VehicleActivityType.TripApplied, "Viagem Almada -> Setubal assumida", "Rota curta com boleia para encontro do grupo.", 48, "km", 4.10, null, 41808, 78, now.AddDays(-7), null, ref changed);
        EnsurePresentationActivity(marta.Id, martaZoe, VehicleActivityType.Charge, "Carga Continente Seixal", "31.0 kWh carregados em posto rápido.", 31.0, "kWh", 13.02, 0.420, 52260, 84, now.AddDays(-3), null, ref changed);
        EnsurePresentationActivity(joao.Id, joaoGolf, VehicleActivityType.Refuel, "Abastecimento Repsol A1", "42.6 L de gasóleo simples para semana de deslocações.", 42.6, "L", 68.12, 1.599, 86820, 96, now.AddDays(-6), null, ref changed);
        EnsurePresentationActivity(joao.Id, joaoGolf, VehicleActivityType.TripApplied, "Viagem Lisboa -> Porto assumida", "Viagem de trabalho com custo partilhado no grupo.", 313, "km", 40.55, null, 87133, 63, now.AddDays(-2), null, ref changed);
        EnsurePresentationActivity(ines.Id, inesKauai, VehicleActivityType.Charge, "Carga Ionity Grandola", "Reforço de 36.5 kWh para chegar ao Algarve com margem.", 36.5, "kWh", 17.89, 0.490, 36540, 78, now.AddDays(-4), null, ref changed);
        EnsurePresentationActivity(rui.Id, ruiSandero, VehicleActivityType.Refuel, "GPL Prio Evora", "31.8 L de GPL registados no posto low-cost.", 31.8, "L", 28.30, 0.890, 27120, 94, now.AddDays(-5), null, ref changed);
        EnsurePresentationActivity(sofia.Id, sofiaBmw, VehicleActivityType.Refuel, "Gasoleo BP Gaia", "48.0 L antes da deslocação Porto-Lisboa.", 48.0, "L", 79.15, 1.649, 119430, 88, now.AddDays(-9), null, ref changed);
        EnsurePresentationActivity(sofia.Id, sofiaBmw, VehicleActivityType.TripApplied, "Viagem Porto -> Lisboa assumida", "Rota longa com previsão de portagens e partilha de custos.", 313, "km", 44.80, null, 119743, 56, now.AddDays(-4), null, ref changed);
        EnsurePresentationActivity(primary.Id, primaryDiesel, VehicleActivityType.Updated, "Dados reais de consumo revistos", "Consumo diesel ajustado com base em histórico de viagens longas.", null, "", null, null, 64210, 68, now.AddHours(-12), null, ref changed);
        EnsurePresentationActivity(primary.Id, primaryEv, VehicleActivityType.Charge, "Carga doméstica noturna", "18.2 kWh carregados durante tarifa bi-horária.", 18.2, "kWh", 3.64, 0.200, 18240, 82, now.AddHours(-8), null, ref changed);

        EnsurePresentationContact(primary.Id, "Marta Costa", marta.Email, ref changed);
        EnsurePresentationContact(primary.Id, "Ines Martins", ines.Email, ref changed);
        EnsurePresentationContact(marta.Id, "Alexandre Miguel", primary.Email, ref changed);
        EnsurePresentationContact(ines.Id, "Alexandre Miguel", primary.Email, ref changed);
        RemovePresentationContact(primary.Id, joao.Email, ref changed);
        RemovePresentationContact(primary.Id, rui.Email, ref changed);
        RemovePresentationContact(primary.Id, sofia.Email, ref changed);
        EnsurePresentationAccessRequest(primary.Id, joao.Id, SocialConnectionStatus.Pending, now.AddMinutes(-52), null, ref changed);
        EnsurePresentationAccessRequest(rui.Id, primary.Id, SocialConnectionStatus.Pending, now.AddMinutes(-34), null, ref changed);
        EnsurePresentationAccessRequest(sofia.Id, primary.Id, SocialConnectionStatus.Declined, now.AddDays(-2), now.AddDays(-1), ref changed);
        EnsurePresentationShare(primary.Id, primaryDiesel.Id, marta.Email, false, ref changed);
        EnsurePresentationShare(primary.Id, primaryEv.Id, ines.Email, false, ref changed);
        EnsurePresentationShare(marta.Id, martaZoe.Id, primary.Email, false, ref changed);

        var activeTrip = EnsurePresentationTrip(
            primary.Id,
            "Caravana Setubal -> Faro",
            primaryDiesel.Id,
            "Setubal",
            "Faro",
            "Galp Palmela Norte",
            now.AddHours(1.5),
            CollaborativeTripStatus.Active,
            18.40m,
            "Viagem ativa com social pulse, mensagens e split de combustível em tempo real.",
            [primary.Email, marta.Email, joao.Email, ines.Email],
            now.AddHours(-2),
            ref changed);
        EnsurePresentationTrip(
            sofia.Id,
            "Porto Tech Ride",
            sofiaBmw.Id,
            "Porto",
            "Lisboa",
            "BP Gaia",
            now.AddDays(2).Date.AddHours(8),
            CollaborativeTripStatus.Scheduled,
            24.75m,
            "Boleia planeada com divisão de gasóleo e portagens.",
            [sofia.Email, primary.Email, joao.Email],
            now.AddDays(-1),
            ref changed);
        EnsurePresentationTrip(
            ines.Id,
            "EV Algarve sem stress",
            inesKauai.Id,
            "Palmela",
            "Faro",
            "Ionity Grandola",
            now.AddDays(5).Date.AddHours(10),
            CollaborativeTripStatus.Scheduled,
            11.90m,
            "Viagem elétrica com paragem planeada para carga e chegada com margem.",
            [ines.Email, primary.Email, marta.Email],
            now.AddHours(-16),
            ref changed);

        EnsurePresentationMessage(marta.Id, marta, primary.Email, primary.Name, "Levo eu a primeira parte até Grandola? O Yaris está com 54%, mas se formos todos talvez compense a tua carrinha.", now.AddHours(-5), null, ref changed);
        EnsurePresentationMessage(primary.Id, primary, marta.Email, marta.Name, "Boa. Vou calcular no Smart Save com diesel e EV para compararmos custo, portagens e autonomia.", now.AddHours(-4.6), null, ref changed);
        EnsurePresentationMessage(joao.Id, joao, primary.Email, primary.Name, "O Golf fez 5.0 L/100 na última Lisboa-Porto. Mete-me no split da viagem se formos pela A2.", now.AddHours(-3.5), null, ref changed);
        EnsurePresentationMessage(ines.Id, ines, primary.Email, primary.Name, "Tenho 38% no Kauai. Se aparecer Ionity ou Continente perto da rota eu alinho na caravana.", now.AddHours(-3), null, ref changed);
        EnsurePresentationMessage(rui.Id, rui, primary.Email, primary.Name, "Se houver Prio com GPL perto da nacional avisa. O Sandero fica ridiculamente barato por km.", now.AddHours(-2.4), null, ref changed);
        EnsurePresentationMessage(sofia.Id, sofia, primary.Email, primary.Name, "Para a plataforma mostra também o meu Porto-Lisboa. Dá jeito ver portagens, consumo e partilha social.", now.AddHours(-1.7), null, ref changed);
        EnsurePresentationMessage(primary.Id, primary, string.Join(", ", activeTrip.Members.Where(email => !email.Equals(primary.Email, StringComparison.OrdinalIgnoreCase))), "Grupo", "Caravana ativa: encontro na Galp Palmela Norte, cada um confirma autonomia antes de arrancar.", now.AddHours(-1), activeTrip.Id, ref changed);
        EnsurePresentationMessage(marta.Id, marta, string.Join(", ", activeTrip.Members.Where(email => !email.Equals(marta.Email, StringComparison.OrdinalIgnoreCase))), "Grupo", "Confirmado. Tenho água, snacks e vou partilhar localização durante a viagem.", now.AddMinutes(-38), activeTrip.Id, ref changed);
        EnsurePresentationMessage(ines.Id, ines, string.Join(", ", activeTrip.Members.Where(email => !email.Equals(ines.Email, StringComparison.OrdinalIgnoreCase))), "Grupo", "O meu EV precisa de reforço em Grandola. O mapa já mostra os carregadores no caminho.", now.AddMinutes(-22), activeTrip.Id, ref changed);

        EnsurePresentationPriceReport(marta.Id, marta.Name, 1.674m, "Galp Palmela Norte estava ligeiramente abaixo do preço médio local.", ref changed);
        EnsurePresentationPriceReport(rui.Id, rui.Name, 0.887m, "GPL confirmado no painel da Prio Evora.", ref changed);
        EnsurePresentationPriceReport(ines.Id, ines.Name, 0.490m, "Ionity Grandola operacional e sem fila.", ref changed);

        if (changed)
        {
            PersistSnapshot();
        }
    }

    private UserAccount EnsurePresentationUser(
        string name,
        string email,
        string password,
        string authProvider,
        string linkedProviders,
        bool isAdmin,
        DateTime lastLoginAtUtc,
        string avatarUrl,
        ref bool changed)
    {
        var user = _users.FirstOrDefault(item => item.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            user = new UserAccount
            {
                Name = name,
                Email = email,
                PasswordHash = PasswordSecurityService.HashPassword(password),
                EmailConfirmed = true,
                AuthProvider = authProvider,
                LinkedProviders = linkedProviders,
                AvatarUrl = avatarUrl,
                Role = isAdmin ? UserRole.Admin : UserRole.User,
                LastLoginAtUtc = lastLoginAtUtc,
                CreatedAtUtc = lastLoginAtUtc.AddDays(-45)
            };
            _users.Add(user);
            changed = true;
            return user;
        }

        changed |= SetIfDifferent(user.Name, name, value => user.Name = value);
        changed |= SetIfDifferent(user.AuthProvider, authProvider, value => user.AuthProvider = value);
        changed |= SetIfDifferent(user.LinkedProviders, linkedProviders, value => user.LinkedProviders = value);
        changed |= SetIfDifferent(user.AvatarUrl, avatarUrl, value => user.AvatarUrl = value);
        changed |= SetIfDifferent(user.EmailConfirmed, true, value => user.EmailConfirmed = value);
        changed |= SetIfDifferent(user.LastLoginAtUtc, (DateTime?)lastLoginAtUtc, value => user.LastLoginAtUtc = value);
        if (string.IsNullOrWhiteSpace(user.PasswordHash)
            || PasswordSecurityService.NeedsRehash(user.PasswordHash)
            || !PasswordSecurityService.VerifyPassword(password, user.PasswordHash))
        {
            user.PasswordHash = PasswordSecurityService.HashPassword(password);
            user.Password = string.Empty;
            changed = true;
        }

        return user;
    }

    private void RemoveInternalValidationUsers(ref bool changed)
    {
        var internalUsers = _users
            .Where(user =>
                user.Email.Contains("mfa.browser.", StringComparison.OrdinalIgnoreCase)
                || user.Email.Contains("teste.mfa.", StringComparison.OrdinalIgnoreCase)
                || user.Name.StartsWith("Teste MFA", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (internalUsers.Count == 0)
        {
            return;
        }

        var internalUserIds = internalUsers.Select(user => user.Id).ToHashSet();
        var internalEmails = internalUsers.Select(user => user.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

        _users.RemoveAll(user => internalUserIds.Contains(user.Id));
        _socialProfiles.RemoveAll(profile => internalUserIds.Contains(profile.UserId));
        _vehicles.RemoveAll(vehicle => internalUserIds.Contains(vehicle.OwnerId));
        _routes.RemoveAll(route => internalUserIds.Contains(route.OwnerId));
        _contacts.RemoveAll(contact => internalUserIds.Contains(contact.OwnerId) || internalEmails.Contains(contact.ContactEmail));
        _connectionRequests.RemoveAll(request => internalUserIds.Contains(request.RequesterId) || internalUserIds.Contains(request.TargetUserId));
        _shares.RemoveAll(share => internalUserIds.Contains(share.OwnerId) || internalEmails.Contains(share.TargetEmail));
        _groups.RemoveAll(group => internalUserIds.Contains(group.OwnerId) || group.Members.Any(internalEmails.Contains));
        _messages.RemoveAll(message =>
            internalUserIds.Contains(message.OwnerId)
            || internalUserIds.Contains(message.SenderUserId)
            || internalEmails.Any(email => message.RecipientEmail.Contains(email, StringComparison.OrdinalIgnoreCase)));
        _collaborativeTrips.RemoveAll(trip => internalUserIds.Contains(trip.OwnerId) || trip.Members.Any(internalEmails.Contains));
        _reports.RemoveAll(report => internalUserIds.Contains(report.UserId));
        _vehicleActivities.RemoveAll(activity => internalUserIds.Contains(activity.OwnerId));
        changed = true;
    }

    private void EnsurePresentationProfile(
        UserAccount user,
        string homeCity,
        string drivingStyle,
        string preferredFuelBrands,
        string bio,
        bool isOpenToCarpool,
        bool shareLiveTripStatus,
        ref bool changed)
    {
        var profile = _socialProfiles.FirstOrDefault(item => item.UserId == user.Id);
        if (profile is null)
        {
            _socialProfiles.Add(new SocialProfile
            {
                UserId = user.Id,
                DisplayName = user.Name,
                HomeCity = homeCity,
                DrivingStyle = drivingStyle,
                PreferredFuelBrands = preferredFuelBrands,
                Bio = bio,
                IsOpenToCarpool = isOpenToCarpool,
                ShareLiveTripStatus = shareLiveTripStatus,
                UpdatedAtUtc = DateTime.UtcNow
            });
            changed = true;
            return;
        }

        changed |= SetIfDifferent(profile.DisplayName, user.Name, value => profile.DisplayName = value);
        changed |= SetIfDifferent(profile.HomeCity, homeCity, value => profile.HomeCity = value);
        changed |= SetIfDifferent(profile.DrivingStyle, drivingStyle, value => profile.DrivingStyle = value);
        changed |= SetIfDifferent(profile.PreferredFuelBrands, preferredFuelBrands, value => profile.PreferredFuelBrands = value);
        changed |= SetIfDifferent(profile.Bio, bio, value => profile.Bio = value);
        changed |= SetIfDifferent(profile.IsOpenToCarpool, isOpenToCarpool, value => profile.IsOpenToCarpool = value);
        changed |= SetIfDifferent(profile.ShareLiveTripStatus, shareLiveTripStatus, value => profile.ShareLiveTripStatus = value);
    }

    private VehicleProfile EnsurePresentationVehicle(
        UserAccount owner,
        string registrationPlate,
        string nickname,
        string brand,
        string model,
        int year,
        FuelKind fuelKind,
        TollClass tollClass,
        double averageConsumption,
        double tankCapacity,
        double? batteryCapacity,
        double averageRangeKm,
        int currentLevelPercent,
        double odometerKm,
        double nextServiceKm,
        bool isDefault,
        DateOnly? lastMaintenance,
        int tireHealthPercent,
        DateOnly? inspectionDueDate,
        DateOnly? insuranceDueDate,
        DateOnly? taxDueDate,
        string notes,
        ref bool changed)
    {
        var vehicle = _vehicles.FirstOrDefault(item =>
            item.OwnerId == owner.Id
            && item.RegistrationPlate.Equals(registrationPlate, StringComparison.OrdinalIgnoreCase));
        if (vehicle is null)
        {
            vehicle = new VehicleProfile
            {
                OwnerId = owner.Id,
                RegistrationPlate = registrationPlate,
                Nickname = nickname,
                Brand = brand,
                Model = model,
                Year = year,
                FuelKind = fuelKind,
                TollClass = tollClass,
                AverageConsumption = averageConsumption,
                TankCapacity = tankCapacity,
                BatteryCapacity = batteryCapacity,
                AverageRangeKm = averageRangeKm,
                CurrentLevelPercent = currentLevelPercent,
                OdometerKm = odometerKm,
                NextServiceKm = nextServiceKm,
                IsDefault = isDefault,
                LastMaintenance = lastMaintenance,
                TireHealthPercent = tireHealthPercent,
                InspectionDueDate = inspectionDueDate,
                InsuranceDueDate = insuranceDueDate,
                TaxDueDate = taxDueDate,
                Notes = notes
            };
            _vehicles.Add(vehicle);
            changed = true;
            return vehicle;
        }

        changed |= SetIfDifferent(vehicle.Nickname, nickname, value => vehicle.Nickname = value);
        changed |= SetIfDifferent(vehicle.Brand, brand, value => vehicle.Brand = value);
        changed |= SetIfDifferent(vehicle.Model, model, value => vehicle.Model = value);
        changed |= SetIfDifferent(vehicle.Year, year, value => vehicle.Year = value);
        changed |= SetIfDifferent(vehicle.FuelKind, fuelKind, value => vehicle.FuelKind = value);
        changed |= SetIfDifferent(vehicle.TollClass, tollClass, value => vehicle.TollClass = value);
        changed |= SetIfDifferent(vehicle.AverageConsumption, averageConsumption, value => vehicle.AverageConsumption = value);
        changed |= SetIfDifferent(vehicle.TankCapacity, tankCapacity, value => vehicle.TankCapacity = value);
        changed |= SetIfDifferent(vehicle.BatteryCapacity, batteryCapacity, value => vehicle.BatteryCapacity = value);
        changed |= SetIfDifferent(vehicle.AverageRangeKm, averageRangeKm, value => vehicle.AverageRangeKm = value);
        changed |= SetIfDifferent(vehicle.CurrentLevelPercent, currentLevelPercent, value => vehicle.CurrentLevelPercent = value);
        changed |= SetIfDifferent(vehicle.OdometerKm, odometerKm, value => vehicle.OdometerKm = value);
        changed |= SetIfDifferent(vehicle.NextServiceKm, (double?)nextServiceKm, value => vehicle.NextServiceKm = value);
        changed |= SetIfDifferent(vehicle.IsDefault, isDefault, value => vehicle.IsDefault = value);
        changed |= SetIfDifferent(vehicle.LastMaintenance, lastMaintenance, value => vehicle.LastMaintenance = value);
        changed |= SetIfDifferent(vehicle.TireHealthPercent, tireHealthPercent, value => vehicle.TireHealthPercent = value);
        changed |= SetIfDifferent(vehicle.InspectionDueDate, inspectionDueDate, value => vehicle.InspectionDueDate = value);
        changed |= SetIfDifferent(vehicle.InsuranceDueDate, insuranceDueDate, value => vehicle.InsuranceDueDate = value);
        changed |= SetIfDifferent(vehicle.TaxDueDate, taxDueDate, value => vehicle.TaxDueDate = value);
        changed |= SetIfDifferent(vehicle.Notes, notes, value => vehicle.Notes = value);
        return vehicle;
    }

    private void EnsurePresentationActivity(
        Guid ownerId,
        VehicleProfile vehicle,
        VehicleActivityType type,
        string title,
        string details,
        double? amount,
        string unit,
        double? costEstimate,
        double? unitPrice,
        double? odometerKm,
        int? levelPercentAfter,
        DateTime createdAt,
        Guid? relatedRouteId,
        ref bool changed)
    {
        if (_vehicleActivities.Any(item => item.OwnerId == ownerId && item.VehicleId == vehicle.Id && item.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _vehicleActivities.Insert(0, new VehicleActivity
        {
            OwnerId = ownerId,
            VehicleId = vehicle.Id,
            RelatedRouteId = relatedRouteId,
            Type = type,
            Title = title,
            Details = details,
            Amount = amount,
            Unit = unit,
            CostEstimate = costEstimate,
            UnitPrice = unitPrice,
            OdometerKm = odometerKm,
            LevelPercentAfter = levelPercentAfter,
            CreatedAt = createdAt
        });
        changed = true;
    }

    private void EnsurePresentationContact(Guid ownerId, string name, string email, ref bool changed)
    {
        if (_contacts.Any(item => item.OwnerId == ownerId && item.ContactEmail.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _contacts.Add(new TrustedContact
        {
            OwnerId = ownerId,
            ContactName = name,
            ContactEmail = email
        });
        changed = true;
    }

    private void RemovePresentationContact(Guid ownerId, string email, ref bool changed)
    {
        var removed = _contacts.RemoveAll(item =>
            item.OwnerId == ownerId
            && item.ContactEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            changed = true;
        }
    }

    private void EnsurePresentationAccessRequest(
        Guid requesterId,
        Guid targetUserId,
        SocialConnectionStatus status,
        DateTime createdAtUtc,
        DateTime? respondedAtUtc,
        ref bool changed)
    {
        var request = _connectionRequests.FirstOrDefault(item =>
            item.RequesterId == requesterId
            && item.TargetUserId == targetUserId);
        if (request is null)
        {
            _connectionRequests.Add(new SocialConnectionRequest
            {
                RequesterId = requesterId,
                TargetUserId = targetUserId,
                Status = status,
                CreatedAtUtc = createdAtUtc,
                RespondedAtUtc = respondedAtUtc
            });
            changed = true;
            return;
        }

        changed |= SetIfDifferent(request.Status, status, value => request.Status = value);
        changed |= SetIfDifferent(request.CreatedAtUtc, createdAtUtc, value => request.CreatedAtUtc = value);
        changed |= SetIfDifferent(request.RespondedAtUtc, respondedAtUtc, value => request.RespondedAtUtc = value);
    }

    private void EnsurePresentationShare(Guid ownerId, Guid vehicleId, string targetEmail, bool canEdit, ref bool changed)
    {
        if (_shares.Any(item =>
                item.OwnerId == ownerId
                && item.VehicleId == vehicleId
                && item.TargetEmail.Equals(targetEmail, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _shares.Add(new SharedVehicle
        {
            OwnerId = ownerId,
            VehicleId = vehicleId,
            TargetEmail = targetEmail,
            CanEdit = canEdit
        });
        changed = true;
    }

    private CollaborativeTrip EnsurePresentationTrip(
        Guid ownerId,
        string name,
        Guid vehicleId,
        string origin,
        string destination,
        string meetingPoint,
        DateTime startsAtUtc,
        CollaborativeTripStatus status,
        decimal estimatedSplitAmount,
        string notes,
        List<string> members,
        DateTime createdAtUtc,
        ref bool changed)
    {
        var trip = _collaborativeTrips.FirstOrDefault(item => item.OwnerId == ownerId && item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (trip is null)
        {
            trip = new CollaborativeTrip
            {
                OwnerId = ownerId,
                VehicleId = vehicleId,
                Name = name,
                Origin = origin,
                Destination = destination,
                MeetingPoint = meetingPoint,
                StartsAtUtc = startsAtUtc,
                Status = status,
                EstimatedSplitAmount = estimatedSplitAmount,
                Notes = notes,
                Members = members.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                CreatedAtUtc = createdAtUtc,
                StartedAtUtc = status == CollaborativeTripStatus.Active ? DateTime.UtcNow.AddMinutes(-28) : null
            };
            _collaborativeTrips.Insert(0, trip);
            changed = true;
            return trip;
        }

        changed |= SetIfDifferent(trip.VehicleId, (Guid?)vehicleId, value => trip.VehicleId = value);
        changed |= SetIfDifferent(trip.Origin, origin, value => trip.Origin = value);
        changed |= SetIfDifferent(trip.Destination, destination, value => trip.Destination = value);
        changed |= SetIfDifferent(trip.MeetingPoint, meetingPoint, value => trip.MeetingPoint = value);
        changed |= SetIfDifferent(trip.StartsAtUtc, (DateTime?)startsAtUtc, value => trip.StartsAtUtc = value);
        changed |= SetIfDifferent(trip.Status, status, value => trip.Status = value);
        changed |= SetIfDifferent(trip.EstimatedSplitAmount, estimatedSplitAmount, value => trip.EstimatedSplitAmount = value);
        changed |= SetIfDifferent(trip.Notes, notes, value => trip.Notes = value);
        var distinctMembers = members.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (!trip.Members.SequenceEqual(distinctMembers, StringComparer.OrdinalIgnoreCase))
        {
            trip.Members = distinctMembers;
            changed = true;
        }

        if (status == CollaborativeTripStatus.Active && trip.StartedAtUtc is null)
        {
            trip.StartedAtUtc = DateTime.UtcNow.AddMinutes(-28);
            changed = true;
        }

        return trip;
    }

    private void EnsurePresentationMessage(
        Guid ownerId,
        UserAccount sender,
        string recipientEmail,
        string recipientName,
        string body,
        DateTime sentAtUtc,
        Guid? relatedTripId,
        ref bool changed)
    {
        if (_messages.Any(item =>
                item.SenderUserId == sender.Id
                && item.Body.Equals(body, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _messages.Insert(0, new DirectMessage
        {
            OwnerId = ownerId,
            SenderUserId = sender.Id,
            SenderName = sender.Name,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            ThreadKey = relatedTripId.HasValue ? $"trip:{relatedTripId}" : BuildThreadKey(sender.Email, recipientEmail),
            Body = body,
            RelatedTripId = relatedTripId,
            SentAtUtc = sentAtUtc
        });
        changed = true;
    }

    private void EnsurePresentationPriceReport(Guid userId, string reporterName, decimal reportedPrice, string note, ref bool changed)
    {
        if (_stations.Count == 0 || _reports.Any(item => item.UserId == userId && item.Note.Equals(note, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var station = _stations
            .OrderBy(item => Math.Abs((decimal)item.Price - reportedPrice))
            .FirstOrDefault();
        if (station is null)
        {
            return;
        }

        _reports.Insert(0, new PriceReport
        {
            StationId = station.Id,
            UserId = userId,
            ReporterName = reporterName,
            ReportedPrice = reportedPrice,
            Note = note,
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        });
        changed = true;
    }

    private static bool SetIfDifferent<T>(T current, T next, Action<T> assign)
    {
        if (EqualityComparer<T>.Default.Equals(current, next))
        {
            return false;
        }

        assign(next);
        return true;
    }

    private void ApplyRouteCompatibilityDefaults()
    {
        foreach (var route in _routes)
        {
            var vehicle = _vehicles.FirstOrDefault(item => item.Id == route.VehicleId);
            if (route.AverageSpeedKmh <= 0)
            {
                route.AverageSpeedKmh = route.DistanceKm > 0 && route.EtaMinutes > 0
                    ? Math.Clamp((int)Math.Round(route.DistanceKm / route.EtaMinutes * 60), 40, 130)
                    : 90;
            }

            if (route.AdjustedConsumptionPer100 <= 0)
            {
                route.AdjustedConsumptionPer100 = vehicle?.AverageConsumption ?? 0;
            }

            if (route.ConsumptionSpeedFactor <= 0)
            {
                route.ConsumptionSpeedFactor = 1;
            }

            if (route.MinimumArrivalLevelPercent <= 0)
            {
                route.MinimumArrivalLevelPercent = vehicle?.FuelKind == FuelKind.Electric ? 12 : 8;
            }
        }
    }

    private void PersistSnapshot(bool includeStations = false)
    {
        if (_dbFactory is null)
        {
            return;
        }

        using var db = _dbFactory.CreateDbContext();
        db.Database.EnsureCreated();
        db.Users.RemoveRange(db.Users);
        db.SocialProfiles.RemoveRange(db.SocialProfiles);
        db.Vehicles.RemoveRange(db.Vehicles);
        if (includeStations)
        {
            db.Stations.RemoveRange(db.Stations);
        }
        db.Routes.RemoveRange(db.Routes);
        db.Contacts.RemoveRange(db.Contacts);
        db.SocialConnectionRequests.RemoveRange(db.SocialConnectionRequests);
        db.SharedVehicles.RemoveRange(db.SharedVehicles);
        db.TravelGroups.RemoveRange(db.TravelGroups);
        db.DirectMessages.RemoveRange(db.DirectMessages);
        db.CollaborativeTrips.RemoveRange(db.CollaborativeTrips);
        db.PriceReports.RemoveRange(db.PriceReports);
        db.VehicleActivities.RemoveRange(db.VehicleActivities);
        db.SyncStatuses.RemoveRange(db.SyncStatuses);
        db.SaveChanges();

        db.Users.AddRange(_users);
        db.SocialProfiles.AddRange(_socialProfiles);
        db.Vehicles.AddRange(_vehicles);
        if (includeStations)
        {
            db.Stations.AddRange(_stations);
        }
        db.Routes.AddRange(_routes);
        db.Contacts.AddRange(_contacts);
        db.SocialConnectionRequests.AddRange(_connectionRequests);
        db.SharedVehicles.AddRange(_shares);
        db.TravelGroups.AddRange(_groups);
        db.DirectMessages.AddRange(_messages);
        db.CollaborativeTrips.AddRange(_collaborativeTrips);
        db.PriceReports.AddRange(_reports);
        db.VehicleActivities.AddRange(_vehicleActivities);
        db.SyncStatuses.AddRange(_syncStatuses.Values);
        db.SaveChanges();
    }

    public UserAccount? ValidateUser(string email, string password)
    {
        lock (_syncRoot)
        {
            var user = _users.FirstOrDefault(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                return null;
            }

            if (user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow)
            {
                return null;
            }

            var validPassword = PasswordSecurityService.VerifyPassword(password, user.PasswordHash)
                                || PasswordSecurityService.VerifyLegacyPassword(password, user.Password);
            if (!validPassword)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(10);
                }

                PersistSnapshot();
                return null;
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEndUtc = null;
            user.LastLoginAtUtc = DateTime.UtcNow;
            user.AuthProvider = string.IsNullOrWhiteSpace(user.AuthProvider) ? "Local" : user.AuthProvider;
            user.LinkedProviders = EnsureProvider(user.LinkedProviders, "Local");
            user.EmailConfirmed = true;
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || PasswordSecurityService.NeedsRehash(user.PasswordHash))
            {
                user.PasswordHash = PasswordSecurityService.HashPassword(password);
                user.Password = string.Empty;
            }

            PersistSnapshot();
            return user;
        }
    }

    public UserAccount? GetUser(Guid id)
        => _users.FirstOrDefault(user => user.Id == id);

    public UserAccount? GetUserByEmail(string email)
        => _users.FirstOrDefault(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public UserAccount Register(string name, string email, string password)
    {
        lock (_syncRoot)
        {
            if (_users.Any(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Ja existe uma conta com esse email.");
            }

            var user = new UserAccount
            {
                Name = name.Trim(),
                Email = email.Trim(),
                PasswordHash = PasswordSecurityService.HashPassword(password),
                Password = string.Empty,
                AuthProvider = "Local",
                LinkedProviders = "Local",
                EmailConfirmed = false
            };
            _users.Add(user);
            _socialProfiles.Add(CreateDefaultSocialProfile(user));
            PersistSnapshot();
            return user;
        }
    }

    public UserAccount FindOrCreateExternalUser(string name, string email, string provider, string externalId, string avatarUrl)
    {
        lock (_syncRoot)
        {
            var user = _users.FirstOrDefault(item => item.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                user = new UserAccount
                {
                    Name = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name.Trim(),
                    Email = email.Trim(),
                    Password = string.Empty,
                    PasswordHash = string.Empty,
                    AuthProvider = provider,
                    ExternalProviderId = externalId,
                    LinkedProviders = provider,
                    AvatarUrl = avatarUrl,
                    EmailConfirmed = true
                };
                _users.Add(user);
                _socialProfiles.Add(CreateDefaultSocialProfile(user));
            }

            user.AuthProvider = provider;
            user.ExternalProviderId = externalId;
            user.LinkedProviders = EnsureProvider(user.LinkedProviders, provider);
            user.AvatarUrl = avatarUrl;
            user.EmailConfirmed = true;
            user.LastLoginAtUtc = DateTime.UtcNow;
            PersistSnapshot();
            return user;
        }
    }

    public DashboardViewModel BuildDashboard(Guid userId)
    {
        var user = GetUser(userId) ?? throw new InvalidOperationException("Utilizador nao encontrado.");
        return new DashboardViewModel
        {
            User = user,
            SocialProfile = GetOrCreateSocialProfile(user),
            Vehicles = _vehicles.Where(vehicle => vehicle.OwnerId == userId).OrderByDescending(vehicle => vehicle.IsDefault).ToList(),
            Stations = _stations.OrderBy(station => station.Price).ToList(),
            Routes = _routes.Where(route => route.OwnerId == userId).OrderByDescending(route => route.CreatedAt).ToList(),
            Contacts = _contacts.Where(contact => contact.OwnerId == userId).ToList(),
            SharedVehicles = _shares.Where(share => share.OwnerId == userId).ToList(),
            Groups = _groups.Where(group => group.OwnerId == userId).ToList(),
            Reports = _reports.OrderByDescending(report => report.CreatedAt).Take(6).ToList(),
            VehicleActivities = _vehicleActivities.Where(activity => activity.OwnerId == userId).OrderByDescending(activity => activity.CreatedAt).ToList(),
            DirectMessages = MessagesForUser(user).OrderByDescending(message => message.SentAtUtc).ToList(),
            CollaborativeTrips = CollaborativeTripsForUser(user).OrderByDescending(trip => trip.CreatedAtUtc).ToList(),
            SocialMembers = _users.Where(member => member.Id != userId && member.Role == UserRole.User).OrderByDescending(member => member.LastLoginAtUtc ?? member.CreatedAtUtc).ToList(),
            SocialDirectory = BuildSocialDirectory(user).ToList(),
            IncomingAccessRequests = BuildSocialDirectory(user)
                .Where(member => member.AccessState == "PendingReceived")
                .ToList(),
            SyncStatuses = new Dictionary<string, ApiSyncStatus>(_syncStatuses),
            AuthenticatorSetup = BuildAuthenticatorSetup(user, false),
            SocialProfileInput = new SocialProfileInputModel
            {
                DisplayName = GetOrCreateSocialProfile(user).DisplayName,
                Bio = GetOrCreateSocialProfile(user).Bio,
                HomeCity = GetOrCreateSocialProfile(user).HomeCity,
                DrivingStyle = GetOrCreateSocialProfile(user).DrivingStyle,
                PreferredFuelBrands = GetOrCreateSocialProfile(user).PreferredFuelBrands,
                IsOpenToCarpool = GetOrCreateSocialProfile(user).IsOpenToCarpool,
                ShareLiveTripStatus = GetOrCreateSocialProfile(user).ShareLiveTripStatus
            }
        };
    }

    public AuthenticatorSetupViewModel GetAuthenticatorSetup(Guid userId)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId) ?? throw new InvalidOperationException("Utilizador nao encontrado.");
            if (string.IsNullOrWhiteSpace(user.AuthenticatorKey))
            {
                user.AuthenticatorKey = _authenticatorService.GenerateSecret();
                PersistSnapshot();
            }

            return BuildAuthenticatorSetup(user, true);
        }
    }

    public bool EnableAuthenticator(Guid userId, string code, out IReadOnlyList<string> recoveryCodes)
    {
        lock (_syncRoot)
        {
            recoveryCodes = [];
            var user = GetUser(userId);
            if (user is null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.AuthenticatorKey))
            {
                user.AuthenticatorKey = _authenticatorService.GenerateSecret();
            }

            if (!_authenticatorService.ValidateCode(user.AuthenticatorKey, code))
            {
                PersistSnapshot();
                return false;
            }

            recoveryCodes = _authenticatorService.GenerateRecoveryCodes();
            user.TwoFactorEnabled = true;
            user.RecoveryCodesJson = JsonSerializer.Serialize(
                recoveryCodes.Select(_authenticatorService.HashRecoveryCode).ToList(),
                JsonOptions);
            PersistSnapshot();
            return true;
        }
    }

    public bool VerifySecondFactor(Guid userId, string code)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId);
            if (user is null || !user.TwoFactorEnabled)
            {
                return false;
            }

            if (_authenticatorService.ValidateCode(user.AuthenticatorKey, code))
            {
                user.LastLoginAtUtc = DateTime.UtcNow;
                PersistSnapshot();
                return true;
            }

            var hashes = ReadRecoveryCodeHashes(user);
            var matchingHash = hashes.FirstOrDefault(hash => _authenticatorService.VerifyRecoveryCode(code, hash));
            if (matchingHash is null)
            {
                return false;
            }

            hashes.Remove(matchingHash);
            user.RecoveryCodesJson = JsonSerializer.Serialize(hashes, JsonOptions);
            user.LastLoginAtUtc = DateTime.UtcNow;
            PersistSnapshot();
            return true;
        }
    }

    public IReadOnlyList<string> RegenerateRecoveryCodes(Guid userId)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId) ?? throw new InvalidOperationException("Utilizador nao encontrado.");
            var recoveryCodes = _authenticatorService.GenerateRecoveryCodes();
            user.RecoveryCodesJson = JsonSerializer.Serialize(
                recoveryCodes.Select(_authenticatorService.HashRecoveryCode).ToList(),
                JsonOptions);
            PersistSnapshot();
            return recoveryCodes;
        }
    }

    public void DisableAuthenticator(Guid userId)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId);
            if (user is null)
            {
                return;
            }

            user.TwoFactorEnabled = false;
            user.AuthenticatorKey = string.Empty;
            user.RecoveryCodesJson = "[]";
            PersistSnapshot();
        }
    }

    public void ResetAuthenticator(Guid userId)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId);
            if (user is null)
            {
                return;
            }

            user.TwoFactorEnabled = false;
            user.AuthenticatorKey = _authenticatorService.GenerateSecret();
            user.RecoveryCodesJson = "[]";
            PersistSnapshot();
        }
    }

    public IReadOnlyList<FuelStation> AllStations() => _stations.OrderBy(station => station.Price).ToList();
    public IReadOnlyList<PlannedRoute> AllRoutes() => _routes.OrderByDescending(route => route.CreatedAt).ToList();
    public IReadOnlyList<PriceReport> AllReports() => _reports.OrderByDescending(report => report.CreatedAt).ToList();
    public IReadOnlyDictionary<string, ApiSyncStatus> SyncStatuses() => new Dictionary<string, ApiSyncStatus>(_syncStatuses);
    public int UserCount() => _users.Count;
    public int VehicleCount() => _vehicles.Count;
    public int RouteCount() => _routes.Count;
    public int ActiveUserCount() => _users.Count(user => user.LastLoginAtUtc >= DateTime.UtcNow.AddHours(-24));
    public int LockedUserCount() => _users.Count(IsLocked);
    public int TwoFactorUserCount() => _users.Count(user => user.TwoFactorEnabled);
    public int PendingSocialRequestCount() => _connectionRequests.Count(request => request.Status == SocialConnectionStatus.Pending);
    public int ActiveTripCount() => _collaborativeTrips.Count(trip => trip.Status == CollaborativeTripStatus.Active);

    public bool IsAdmin(Guid? userId)
        => userId.HasValue && GetUser(userId.Value)?.Role == UserRole.Admin;

    public IReadOnlyList<AdminUserSummaryViewModel> BuildAdminUserSummaries()
    {
        lock (_syncRoot)
        {
            return _users
                .OrderByDescending(user => user.Role == UserRole.Admin)
                .ThenByDescending(user => user.LastLoginAtUtc ?? user.CreatedAtUtc)
                .Select(user => new AdminUserSummaryViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsLocked = IsLocked(user),
                    AccessMode = BuildAccessModeLabel(user),
                    FailedLoginAttempts = user.FailedLoginAttempts,
                    VehicleCount = _vehicles.Count(vehicle => vehicle.OwnerId == user.Id),
                    RouteCount = _routes.Count(route => route.OwnerId == user.Id),
                    MessageCount = CountMessagesForAdmin(user),
                    TripCount = CollaborativeTripsForUser(user).Count(),
                    CreatedAtUtc = user.CreatedAtUtc,
                    LastLoginAtUtc = user.LastLoginAtUtc
                })
                .ToList();
        }
    }

    public IReadOnlyList<AdminRouteSummaryViewModel> BuildAdminRouteSummaries(int take = 8)
    {
        lock (_syncRoot)
        {
            return _routes
                .OrderByDescending(route => route.CreatedAt)
                .Take(take)
                .Select(route =>
                {
                    var owner = GetUser(route.OwnerId);
                    var vehicle = _vehicles.FirstOrDefault(item => item.Id == route.VehicleId);
                    return new AdminRouteSummaryViewModel
                    {
                        RouteId = route.Id,
                        OwnerName = owner?.Name ?? "Conta removida",
                        VehicleName = vehicle is null ? "Veiculo nao associado" : $"{vehicle.Brand} {vehicle.Model}",
                        Origin = route.Origin,
                        Destination = route.Destination,
                        DistanceKm = route.DistanceKm,
                        CheapestCost = route.CheapestCost,
                        EstimatedArrivalLevelPercent = route.EstimatedArrivalLevelPercent,
                        RequiresIntermediateStops = route.RequiresIntermediateStops,
                        IsAppliedToVehicle = route.IsAppliedToVehicle,
                        CreatedAt = route.CreatedAt
                    };
                })
                .ToList();
        }
    }

    public IReadOnlyList<AdminTripSummaryViewModel> BuildAdminTripSummaries(int take = 6)
    {
        lock (_syncRoot)
        {
            return _collaborativeTrips
                .OrderByDescending(trip => trip.Status == CollaborativeTripStatus.Active)
                .ThenByDescending(trip => trip.CreatedAtUtc)
                .Take(take)
                .Select(trip => new AdminTripSummaryViewModel
                {
                    TripId = trip.Id,
                    Name = trip.Name,
                    OwnerName = GetUser(trip.OwnerId)?.Name ?? "Conta removida",
                    Origin = trip.Origin,
                    Destination = trip.Destination,
                    Status = trip.Status,
                    MemberCount = trip.Members.Count,
                    EstimatedSplitAmount = trip.EstimatedSplitAmount,
                    StartsAtUtc = trip.StartsAtUtc
                })
                .ToList();
        }
    }

    public bool TrySetUserRole(Guid actingAdminId, Guid targetUserId, UserRole role, out string message)
    {
        lock (_syncRoot)
        {
            message = string.Empty;
            if (!IsAdmin(actingAdminId))
            {
                message = "Apenas administradores podem alterar permissoes.";
                return false;
            }

            var target = GetUser(targetUserId);
            if (target is null)
            {
                message = "Utilizador nao encontrado.";
                return false;
            }

            if (target.Id == actingAdminId && role != UserRole.Admin)
            {
                message = "Nao podes remover o teu proprio acesso de administrador.";
                return false;
            }

            if (target.Role == UserRole.Admin && role != UserRole.Admin && _users.Count(user => user.Role == UserRole.Admin) <= 1)
            {
                message = "A plataforma precisa de manter pelo menos um administrador.";
                return false;
            }

            target.Role = role;
            PersistSnapshot();
            message = role == UserRole.Admin
                ? $"{target.Name} passou a administrador."
                : $"{target.Name} passou a utilizador normal.";
            return true;
        }
    }

    public bool TrySetUserLock(Guid actingAdminId, Guid targetUserId, bool locked, out string message)
    {
        lock (_syncRoot)
        {
            message = string.Empty;
            if (!IsAdmin(actingAdminId))
            {
                message = "Apenas administradores podem gerir bloqueios.";
                return false;
            }

            var target = GetUser(targetUserId);
            if (target is null)
            {
                message = "Utilizador nao encontrado.";
                return false;
            }

            if (target.Id == actingAdminId)
            {
                message = "Nao podes bloquear a tua propria conta.";
                return false;
            }

            target.LockoutEndUtc = locked ? DateTime.UtcNow.AddYears(1) : null;
            if (!locked)
            {
                target.FailedLoginAttempts = 0;
            }

            PersistSnapshot();
            message = locked ? $"{target.Name} foi bloqueado." : $"{target.Name} foi desbloqueado.";
            return true;
        }
    }

    public bool TryConfirmUserEmail(Guid actingAdminId, Guid targetUserId, out string message)
    {
        lock (_syncRoot)
        {
            message = string.Empty;
            if (!IsAdmin(actingAdminId))
            {
                message = "Apenas administradores podem confirmar contas.";
                return false;
            }

            var target = GetUser(targetUserId);
            if (target is null)
            {
                message = "Utilizador nao encontrado.";
                return false;
            }

            target.EmailConfirmed = true;
            PersistSnapshot();
            message = $"Conta de {target.Name} confirmada.";
            return true;
        }
    }

    public bool TryResetUserMfa(Guid actingAdminId, Guid targetUserId, out string message)
    {
        lock (_syncRoot)
        {
            message = string.Empty;
            if (!IsAdmin(actingAdminId))
            {
                message = "Apenas administradores podem repor MFA.";
                return false;
            }

            var target = GetUser(targetUserId);
            if (target is null)
            {
                message = "Utilizador nao encontrado.";
                return false;
            }

            target.TwoFactorEnabled = false;
            target.AuthenticatorKey = _authenticatorService.GenerateSecret();
            target.RecoveryCodesJson = "[]";
            PersistSnapshot();
            message = $"MFA de {target.Name} foi reposto. A conta pode configurar novamente o Authenticator.";
            return true;
        }
    }

    public Guid AddVehicle(Guid userId, VehicleInputModel input)
    {
        lock (_syncRoot)
        {
            if (_vehicles.All(vehicle => vehicle.OwnerId != userId))
            {
                foreach (var existingVehicle in _vehicles.Where(vehicle => vehicle.OwnerId == userId))
                {
                    existingVehicle.IsDefault = false;
                }
            }

            var vehicle = new VehicleProfile
            {
                OwnerId = userId,
                Nickname = input.Nickname,
                RegistrationPlate = (input.RegistrationPlate ?? string.Empty).Trim().ToUpperInvariant(),
                Brand = input.Brand.Trim(),
                Model = input.Model.Trim(),
                Year = input.Year,
                FuelKind = input.FuelKind,
                TollClass = input.TollClass,
                AverageConsumption = input.AverageConsumption,
                TankCapacity = input.TankCapacity,
                BatteryCapacity = input.BatteryCapacity,
                AverageRangeKm = input.AverageRangeKm,
                CurrentLevelPercent = input.CurrentLevelPercent,
                OdometerKm = input.OdometerKm,
                NextServiceKm = input.NextServiceKm ?? (input.OdometerKm > 0 ? input.OdometerKm + 15000 : 15000),
                LastMaintenance = input.LastMaintenance,
                TireHealthPercent = Math.Clamp(input.TireHealthPercent, 0, 100),
                InspectionDueDate = input.InspectionDueDate,
                InsuranceDueDate = input.InsuranceDueDate,
                TaxDueDate = input.TaxDueDate,
                Notes = (input.Notes ?? string.Empty).Trim(),
                IsDefault = !_vehicles.Any(vehicle => vehicle.OwnerId == userId && vehicle.IsDefault)
            };

            _vehicles.Add(vehicle);
            AddVehicleActivity(
                userId,
                vehicle,
                VehicleActivityType.Created,
                "Veículo adicionado à garagem",
                $"{vehicle.Brand} {vehicle.Model} guardado com {vehicle.CurrentLevelPercent}% de nível inicial.",
                odometerKm: vehicle.OdometerKm,
                levelPercentAfter: vehicle.CurrentLevelPercent);
            PersistSnapshot();
            return vehicle.Id;
        }
    }

    public void RemoveVehicle(Guid userId, Guid vehicleId)
    {
        lock (_syncRoot)
        {
            var vehicle = _vehicles.FirstOrDefault(item => item.Id == vehicleId && item.OwnerId == userId);
            if (vehicle is null) return;
            _vehicles.Remove(vehicle);
            _vehicleActivities.RemoveAll(activity => activity.OwnerId == userId && activity.VehicleId == vehicleId);
            var fallback = _vehicles.FirstOrDefault(item => item.OwnerId == userId);
            if (fallback is not null)
            {
                fallback.IsDefault = true;
            }

            PersistSnapshot();
        }
    }

    public void SetDefaultVehicle(Guid userId, Guid vehicleId)
    {
        lock (_syncRoot)
        {
            foreach (var vehicle in _vehicles.Where(vehicle => vehicle.OwnerId == userId))
            {
                vehicle.IsDefault = vehicle.Id == vehicleId;
            }

            PersistSnapshot();
        }
    }

    public VehicleProfile? GetVehicle(Guid userId, Guid vehicleId)
        => _vehicles.FirstOrDefault(vehicle => vehicle.OwnerId == userId && vehicle.Id == vehicleId);

    public void UpdateVehicleGarageData(Guid userId, VehicleStatusInputModel input)
    {
        lock (_syncRoot)
        {
            var vehicle = _vehicles.FirstOrDefault(item => item.OwnerId == userId && item.Id == input.Id);
            if (vehicle is null)
            {
                return;
            }

            vehicle.CurrentLevelPercent = Math.Clamp(input.CurrentLevelPercent, 0, 100);
            vehicle.Nickname = input.Nickname.Trim();
            vehicle.RegistrationPlate = (input.RegistrationPlate ?? string.Empty).Trim().ToUpperInvariant();
            vehicle.Brand = input.Brand.Trim();
            vehicle.Model = input.Model.Trim();
            vehicle.Year = input.Year;
            vehicle.FuelKind = input.FuelKind;
            vehicle.TollClass = input.TollClass;
            vehicle.AverageConsumption = Math.Max(0.1, input.AverageConsumption);
            vehicle.AverageRangeKm = Math.Max(1, input.AverageRangeKm);
            vehicle.TankCapacity = Math.Max(0, input.TankCapacity);
            vehicle.BatteryCapacity = input.BatteryCapacity.HasValue ? Math.Max(0, input.BatteryCapacity.Value) : null;
            vehicle.OdometerKm = Math.Max(0, input.OdometerKm);
            vehicle.NextServiceKm = input.NextServiceKm.HasValue ? Math.Max(0, input.NextServiceKm.Value) : null;
            vehicle.LastMaintenance = input.LastMaintenance;
            vehicle.TireHealthPercent = Math.Clamp(input.TireHealthPercent, 0, 100);
            vehicle.InspectionDueDate = input.InspectionDueDate;
            vehicle.InsuranceDueDate = input.InsuranceDueDate;
            vehicle.TaxDueDate = input.TaxDueDate;
            vehicle.Notes = (input.Notes ?? string.Empty).Trim();

            AddVehicleActivity(
                userId,
                vehicle,
                VehicleActivityType.Updated,
                "Ficha do veículo atualizada",
                $"Dados base, consumo e manutenção de {vehicle.Nickname} foram atualizados.",
                odometerKm: vehicle.OdometerKm,
                levelPercentAfter: vehicle.CurrentLevelPercent);
            PersistSnapshot();
        }
    }

    public void RefuelVehicle(Guid userId, RefuelVehicleInputModel input)
    {
        lock (_syncRoot)
        {
            var vehicle = _vehicles.FirstOrDefault(item => item.OwnerId == userId && item.Id == input.Id);
            if (vehicle is null)
            {
                return;
            }

            if (input.FillToFull)
            {
                var fullAmount = Math.Max(0, capacityForVehicle(vehicle) * (100 - vehicle.CurrentLevelPercent) / 100d);
                var totalCost = input.UnitPrice.HasValue ? Math.Round(fullAmount * input.UnitPrice.Value, 2) : (double?)null;
                vehicle.CurrentLevelPercent = 100;
                AddVehicleActivity(
                    userId,
                    vehicle,
                    vehicle.FuelKind == FuelKind.Electric ? VehicleActivityType.Charge : VehicleActivityType.Refuel,
                    vehicle.FuelKind == FuelKind.Electric ? "Carga completa registada" : "Depósito cheio registado",
                    BuildFuelEntryDetails(vehicle, input.StationName, fullAmount, input.UnitPrice, true),
                    amount: Math.Round(fullAmount, 1),
                    unit: vehicle.FuelKind == FuelKind.Electric ? "kWh" : "L",
                    costEstimate: totalCost,
                    unitPrice: input.UnitPrice,
                    odometerKm: vehicle.OdometerKm,
                    levelPercentAfter: vehicle.CurrentLevelPercent);
                PersistSnapshot();
                return;
            }

            var capacity = capacityForVehicle(vehicle);

            if (capacity <= 0 || input.Amount <= 0)
            {
                return;
            }

            var deltaPercent = input.Amount / capacity * 100d;
            vehicle.CurrentLevelPercent = Math.Clamp((int)Math.Round(vehicle.CurrentLevelPercent + deltaPercent), 0, 100);
            var costEstimate = input.UnitPrice.HasValue ? Math.Round(input.Amount * input.UnitPrice.Value, 2) : (double?)null;
            AddVehicleActivity(
                userId,
                vehicle,
                vehicle.FuelKind == FuelKind.Electric ? VehicleActivityType.Charge : VehicleActivityType.Refuel,
                vehicle.FuelKind == FuelKind.Electric ? "Carregamento registado" : "Abastecimento registado",
                BuildFuelEntryDetails(vehicle, input.StationName, input.Amount, input.UnitPrice, false),
                amount: Math.Round(input.Amount, 1),
                unit: vehicle.FuelKind == FuelKind.Electric ? "kWh" : "L",
                costEstimate: costEstimate,
                unitPrice: input.UnitPrice,
                odometerKm: vehicle.OdometerKm,
                levelPercentAfter: vehicle.CurrentLevelPercent);
            PersistSnapshot();
        }
    }

    public (bool Success, string Message) ApplyRouteToVehicle(Guid userId, Guid routeId)
    {
        lock (_syncRoot)
        {
            var route = _routes.FirstOrDefault(item => item.Id == routeId && item.OwnerId == userId);
            if (route is null)
            {
                return (false, "Viagem não encontrada.");
            }

            if (route.IsAppliedToVehicle)
            {
                return (false, "Esta viagem já foi assumida na garagem.");
            }

            var vehicle = _vehicles.FirstOrDefault(item => item.Id == route.VehicleId && item.OwnerId == userId);
            if (vehicle is null)
            {
                return (false, "Veículo da viagem não encontrado.");
            }

            var consumptionAmount = route.DistanceKm / 100d * vehicle.AverageConsumption;
            var capacity = vehicle.FuelKind == FuelKind.Electric
                ? vehicle.BatteryCapacity ?? 0
                : vehicle.TankCapacity;
            var stopPlans = ReadSuggestedStops(route);
            var replenishedAmount = stopPlans.Sum(stop => Math.Max(0, stop.SuggestedAmount));

            if (capacity > 0)
            {
                var currentAmount = capacity * vehicle.CurrentLevelPercent / 100d;
                var finalAmount = Math.Clamp(currentAmount + replenishedAmount - consumptionAmount, 0, capacity);
                vehicle.CurrentLevelPercent = Math.Clamp((int)Math.Round(finalAmount / capacity * 100d), 0, 100);
            }

            vehicle.OdometerKm += route.DistanceKm;
            route.IsAppliedToVehicle = true;
            route.AppliedAtUtc = DateTime.UtcNow;

            var unit = vehicle.FuelKind == FuelKind.Electric ? "kWh" : "L";
            AddVehicleActivity(
                userId,
                vehicle,
                VehicleActivityType.TripApplied,
                $"Viagem {route.Origin} -> {route.Destination} assumida",
                stopPlans.Count == 0
                    ? $"{route.DistanceKm:0.#} km executados. Consumo estimado {consumptionAmount:0.0} {unit}."
                    : $"{route.DistanceKm:0.#} km executados. Consumo {consumptionAmount:0.0} {unit}, com {stopPlans.Count} paragem(ns) e +{replenishedAmount:0.0} {unit} planeados.",
                amount: route.DistanceKm,
                unit: "km",
                costEstimate: route.CheapestCost,
                odometerKm: vehicle.OdometerKm,
                levelPercentAfter: vehicle.CurrentLevelPercent,
                relatedRouteId: route.Id);
            PersistSnapshot();
            return stopPlans.Count == 0
                ? (true, $"Viagem assumida. Foram descontados cerca de {consumptionAmount:0.0} {unit}.")
                : (true, $"Viagem assumida. Consumo estimado: {consumptionAmount:0.0} {unit}, com {stopPlans.Count} paragem(ns) intermédia(s) e +{replenishedAmount:0.0} {unit} registados.");
        }
    }

    private static IReadOnlyList<RouteStopPlan> ReadSuggestedStops(PlannedRoute route)
    {
        if (string.IsNullOrWhiteSpace(route.SuggestedStopsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<RouteStopPlan>>(route.SuggestedStopsJson, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void SaveRoute(PlannedRoute route)
    {
        lock (_syncRoot)
        {
            _routes.Insert(0, route);
            PersistSnapshot();
        }
    }

    public void AddContact(Guid userId, ContactInputModel input)
    {
        lock (_syncRoot)
        {
            var target = input.TargetUserId.HasValue ? GetUser(input.TargetUserId.Value) : GetUserByEmail(input.Email);
            var contactEmail = target?.Email ?? input.Email.Trim();
            var contactName = target?.Name ?? input.Name.Trim();
            if (string.IsNullOrWhiteSpace(contactEmail) || string.IsNullOrWhiteSpace(contactName))
            {
                return;
            }

            if (AddTrustedContactIfMissing(userId, contactName, contactEmail))
            {
                PersistSnapshot();
            }
        }
    }

    public bool RequestSocialAccess(Guid userId, Guid targetUserId)
    {
        lock (_syncRoot)
        {
            var requester = GetUser(userId);
            var target = GetUser(targetUserId);
            if (requester is null || target is null || requester.Id == target.Id || AreTrustedContacts(requester, target))
            {
                return false;
            }

            var reverseRequest = _connectionRequests.FirstOrDefault(request =>
                request.RequesterId == target.Id
                && request.TargetUserId == requester.Id
                && request.Status == SocialConnectionStatus.Pending);
            if (reverseRequest is not null)
            {
                AcceptSocialAccessLocked(requester.Id, reverseRequest);
                PersistSnapshot();
                return true;
            }

            if (_connectionRequests.Any(request =>
                    request.RequesterId == requester.Id
                    && request.TargetUserId == target.Id
                    && request.Status == SocialConnectionStatus.Pending))
            {
                return false;
            }

            _connectionRequests.Insert(0, new SocialConnectionRequest
            {
                RequesterId = requester.Id,
                TargetUserId = target.Id,
                Status = SocialConnectionStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            });
            PersistSnapshot();
            return true;
        }
    }

    public bool AcceptSocialAccess(Guid userId, Guid requestId)
    {
        lock (_syncRoot)
        {
            var request = _connectionRequests.FirstOrDefault(item =>
                item.Id == requestId
                && item.TargetUserId == userId
                && item.Status == SocialConnectionStatus.Pending);
            if (request is null)
            {
                return false;
            }

            AcceptSocialAccessLocked(userId, request);
            PersistSnapshot();
            return true;
        }
    }

    public bool DeclineSocialAccess(Guid userId, Guid requestId)
    {
        lock (_syncRoot)
        {
            var request = _connectionRequests.FirstOrDefault(item =>
                item.Id == requestId
                && item.TargetUserId == userId
                && item.Status == SocialConnectionStatus.Pending);
            if (request is null)
            {
                return false;
            }

            request.Status = SocialConnectionStatus.Declined;
            request.RespondedAtUtc = DateTime.UtcNow;
            PersistSnapshot();
            return true;
        }
    }

    public void ShareVehicle(Guid userId, ShareVehicleInputModel input)
    {
        lock (_syncRoot)
        {
            var target = input.TargetUserId.HasValue ? GetUser(input.TargetUserId.Value) : GetUserByEmail(input.TargetEmail);
            var targetEmail = target?.Email ?? input.TargetEmail.Trim();
            if (string.IsNullOrWhiteSpace(targetEmail))
            {
                return;
            }

            _shares.Add(new SharedVehicle
            {
                OwnerId = userId,
                VehicleId = input.VehicleId,
                TargetEmail = targetEmail,
                CanEdit = input.CanEdit
            });
            PersistSnapshot();
        }
    }

    public void AddGroup(Guid userId, GroupInputModel input)
    {
        lock (_syncRoot)
        {
            _groups.Add(new TravelGroup
            {
                OwnerId = userId,
                Name = input.Name,
                Description = input.Description,
                SplitAmount = input.SplitAmount,
                Members = input.MemberEmails
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Prepend(GetUser(userId)?.Email ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            });
            PersistSnapshot();
        }
    }

    public void AddReport(Guid userId, PriceReportInputModel input)
    {
        lock (_syncRoot)
        {
            var reporter = GetUser(userId);
            if (reporter is null) return;
            _reports.Insert(0, new PriceReport
            {
                UserId = userId,
                StationId = input.StationId,
                ReporterName = reporter.Name,
                ReportedPrice = input.ReportedPrice,
                Note = input.Note
            });
            PersistSnapshot();
        }
    }

    public void UpdateSocialProfile(Guid userId, SocialProfileInputModel input)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId);
            if (user is null)
            {
                return;
            }

            var profile = GetOrCreateSocialProfile(user);
            profile.DisplayName = input.DisplayName.Trim();
            profile.Bio = input.Bio.Trim();
            profile.HomeCity = input.HomeCity.Trim();
            profile.DrivingStyle = input.DrivingStyle.Trim();
            profile.PreferredFuelBrands = input.PreferredFuelBrands.Trim();
            profile.IsOpenToCarpool = input.IsOpenToCarpool;
            profile.ShareLiveTripStatus = input.ShareLiveTripStatus;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            user.Name = profile.DisplayName;
            PersistSnapshot();
        }
    }

    public void SendDirectMessage(Guid userId, DirectMessageInputModel input)
    {
        lock (_syncRoot)
        {
            var sender = GetUser(userId);
            if (sender is null || string.IsNullOrWhiteSpace(input.Body))
            {
                return;
            }

            var recipient = input.RecipientUserId.HasValue ? GetUser(input.RecipientUserId.Value) : GetUserByEmail(input.RecipientEmail);
            var recipientEmail = recipient?.Email ?? input.RecipientEmail.Trim();
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                return;
            }

            _messages.Insert(0, new DirectMessage
            {
                OwnerId = sender.Id,
                SenderUserId = sender.Id,
                SenderName = sender.Name,
                RecipientEmail = recipientEmail,
                RecipientName = recipient?.Name ?? recipientEmail,
                ThreadKey = BuildThreadKey(sender.Email, recipientEmail),
                Body = input.Body.Trim(),
                RelatedTripId = input.RelatedTripId,
                SentAtUtc = DateTime.UtcNow
            });
            PersistSnapshot();
        }
    }

    public Guid CreateCollaborativeTrip(Guid userId, CollaborativeTripInputModel input)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId) ?? throw new InvalidOperationException("Utilizador nao encontrado.");
            var selectedRoute = input.RouteId.HasValue
                ? _routes.FirstOrDefault(route => route.Id == input.RouteId && route.OwnerId == userId)
                : null;
            var selectedVehicle = input.VehicleId.HasValue
                ? _vehicles.FirstOrDefault(vehicle => vehicle.Id == input.VehicleId && vehicle.OwnerId == userId)
                : null;
            var memberEmails = input.MemberUserIds
                .Select(GetUser)
                .Where(member => member is not null)
                .Select(member => member!.Email)
                .Concat(input.MemberEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Append(user.Email)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var trip = new CollaborativeTrip
            {
                OwnerId = userId,
                RouteId = selectedRoute?.Id ?? input.RouteId,
                VehicleId = selectedVehicle?.Id ?? input.VehicleId,
                Name = input.Name.Trim(),
                Origin = !string.IsNullOrWhiteSpace(input.Origin) ? input.Origin.Trim() : selectedRoute?.Origin ?? string.Empty,
                Destination = !string.IsNullOrWhiteSpace(input.Destination) ? input.Destination.Trim() : selectedRoute?.Destination ?? string.Empty,
                MeetingPoint = input.MeetingPoint.Trim(),
                StartsAtUtc = input.StartsAtLocal?.ToUniversalTime(),
                Status = input.StartsAtLocal.HasValue ? CollaborativeTripStatus.Scheduled : CollaborativeTripStatus.Planned,
                Members = memberEmails,
                EstimatedSplitAmount = input.EstimatedSplitAmount,
                Notes = input.Notes.Trim()
            };

            _collaborativeTrips.Insert(0, trip);
            _messages.Insert(0, new DirectMessage
            {
                OwnerId = userId,
                SenderUserId = userId,
                SenderName = user.Name,
                RecipientEmail = string.Join(", ", memberEmails.Where(email => !email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))),
                RecipientName = "Grupo",
                ThreadKey = $"trip:{trip.Id}",
                Body = $"Viagem colaborativa criada: {trip.Name}. Ponto de encontro: {trip.MeetingPoint}.",
                RelatedTripId = trip.Id,
                SentAtUtc = DateTime.UtcNow
            });
            PersistSnapshot();
            return trip.Id;
        }
    }

    public void JoinCollaborativeTrip(Guid userId, Guid tripId)
    {
        lock (_syncRoot)
        {
            var user = GetUser(userId);
            var trip = _collaborativeTrips.FirstOrDefault(item => item.Id == tripId);
            if (user is null || trip is null)
            {
                return;
            }

            if (!trip.Members.Contains(user.Email, StringComparer.OrdinalIgnoreCase))
            {
                trip.Members.Add(user.Email);
                PersistSnapshot();
            }
        }
    }

    public void StartCollaborativeTrip(Guid userId, Guid tripId)
    {
        lock (_syncRoot)
        {
            var trip = _collaborativeTrips.FirstOrDefault(item => item.Id == tripId && (item.OwnerId == userId || item.Members.Contains(GetUser(userId)?.Email ?? string.Empty, StringComparer.OrdinalIgnoreCase)));
            if (trip is null)
            {
                return;
            }

            trip.Status = CollaborativeTripStatus.Active;
            trip.StartedAtUtc = DateTime.UtcNow;
            _messages.Insert(0, new DirectMessage
            {
                OwnerId = trip.OwnerId,
                SenderUserId = userId,
                SenderName = GetUser(userId)?.Name ?? "Participante",
                RecipientEmail = string.Join(", ", trip.Members),
                RecipientName = "Grupo",
                ThreadKey = $"trip:{trip.Id}",
                Body = $"Viagem iniciada em grupo: {trip.Name}.",
                RelatedTripId = trip.Id,
                SentAtUtc = DateTime.UtcNow
            });
            PersistSnapshot();
        }
    }

    public void RefreshStationPrices()
    {
        lock (_syncRoot)
        {
            var random = new Random();
            foreach (var station in _stations)
            {
                var delta = random.NextDouble() * 0.06 - 0.03;
                station.Price = Math.Round(Math.Max(0.29, station.Price + delta), 3);
                station.LastUpdatedAt = DateTime.UtcNow;
            }

            PersistSnapshot(includeStations: true);
        }
    }

    public ApiSyncStatus StartSyncAttempt(string key)
    {
        lock (_syncRoot)
        {
            if (!_syncStatuses.TryGetValue(key, out var status))
            {
                status = new ApiSyncStatus { SourceName = key };
                _syncStatuses[key] = status;
            }

            status.LastAttemptUtc = DateTime.UtcNow;
            status.Message = "Sincronizacao em curso...";
            return status;
        }
    }

    public void CompleteSyncAttempt(string key, bool success, int itemCount, string message)
    {
        lock (_syncRoot)
        {
            if (!_syncStatuses.TryGetValue(key, out var status))
            {
                status = new ApiSyncStatus { SourceName = key };
                _syncStatuses[key] = status;
            }

            status.IsSuccess = success;
            status.ItemCount = itemCount;
            status.Message = message;
            status.LastAttemptUtc ??= DateTime.UtcNow;
            if (success)
            {
                status.LastSuccessUtc = DateTime.UtcNow;
            }

            PersistSnapshot();
        }
    }

    public void ReplaceStations(IReadOnlyList<FuelStation> stations, string source)
    {
        lock (_syncRoot)
        {
            _stations.Clear();
            foreach (var station in stations)
            {
                station.Source = source;
                _stations.Add(station);
            }

            PersistSnapshot(includeStations: true);
        }
    }

    private IEnumerable<SocialMemberCardViewModel> BuildSocialDirectory(UserAccount currentUser)
    {
        var onlineThreshold = DateTime.UtcNow.AddHours(-24);
        return _users
            .Where(member => member.Id != currentUser.Id && member.Role == UserRole.User)
            .Select(member =>
            {
                var profile = GetOrCreateSocialProfile(member);
                var request = _connectionRequests
                    .Where(item =>
                        (item.RequesterId == currentUser.Id && item.TargetUserId == member.Id)
                        || (item.RequesterId == member.Id && item.TargetUserId == currentUser.Id))
                    .OrderByDescending(item => item.CreatedAtUtc)
                    .FirstOrDefault();
                var isConnected = AreTrustedContacts(currentUser, member) || request?.Status == SocialConnectionStatus.Accepted;
                var accessState = isConnected
                    ? "Connected"
                    : request is { Status: SocialConnectionStatus.Pending } && request.RequesterId == currentUser.Id
                        ? "PendingSent"
                        : request is { Status: SocialConnectionStatus.Pending } && request.TargetUserId == currentUser.Id
                            ? "PendingReceived"
                            : "None";

                return new SocialMemberCardViewModel
                {
                    UserId = member.Id,
                    DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? member.Name : profile.DisplayName,
                    Initials = BuildInitials(string.IsNullOrWhiteSpace(profile.DisplayName) ? member.Name : profile.DisplayName),
                    AvatarUrl = member.AvatarUrl,
                    Bio = profile.Bio,
                    HomeCity = profile.HomeCity,
                    DrivingStyle = profile.DrivingStyle,
                    PreferredFuelBrands = profile.PreferredFuelBrands,
                    IsOnline = member.LastLoginAtUtc is not null && member.LastLoginAtUtc >= onlineThreshold,
                    IsOpenToCarpool = profile.IsOpenToCarpool,
                    ShareLiveTripStatus = profile.ShareLiveTripStatus,
                    VehicleCount = _vehicles.Count(vehicle => vehicle.OwnerId == member.Id),
                    SharedTripCount = _collaborativeTrips.Count(trip =>
                        trip.Members.Contains(currentUser.Email, StringComparer.OrdinalIgnoreCase)
                        && trip.Members.Contains(member.Email, StringComparer.OrdinalIgnoreCase)),
                    AccessState = accessState,
                    RequestId = request?.Id
                };
            })
            .OrderByDescending(member => member.IsOnline)
            .ThenBy(member => member.AccessState == "Connected" ? 0 : member.AccessState == "PendingReceived" ? 1 : member.AccessState == "PendingSent" ? 2 : 3)
            .ThenBy(member => member.DisplayName);
    }

    private bool AreTrustedContacts(UserAccount currentUser, UserAccount member)
        => _contacts.Any(contact =>
            contact.OwnerId == currentUser.Id
            && contact.ContactEmail.Equals(member.Email, StringComparison.OrdinalIgnoreCase));

    private bool AddTrustedContactIfMissing(Guid ownerId, string contactName, string contactEmail)
    {
        if (_contacts.Any(contact => contact.OwnerId == ownerId && contact.ContactEmail.Equals(contactEmail, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        _contacts.Add(new TrustedContact
        {
            OwnerId = ownerId,
            ContactName = contactName,
            ContactEmail = contactEmail
        });
        return true;
    }

    private void AcceptSocialAccessLocked(Guid userId, SocialConnectionRequest request)
    {
        var requester = GetUser(request.RequesterId);
        var target = GetUser(request.TargetUserId);
        if (requester is null || target is null || target.Id != userId)
        {
            return;
        }

        request.Status = SocialConnectionStatus.Accepted;
        request.RespondedAtUtc = DateTime.UtcNow;
        AddTrustedContactIfMissing(requester.Id, target.Name, target.Email);
        AddTrustedContactIfMissing(target.Id, requester.Name, requester.Email);
    }

    private static string BuildInitials(string name)
    {
        var initials = string.Concat(name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part[0])
            .Take(2));
        return string.IsNullOrWhiteSpace(initials) ? "OD" : initials.ToUpperInvariant();
    }

    private SocialProfile GetOrCreateSocialProfile(UserAccount user)
    {
        var profile = _socialProfiles.FirstOrDefault(item => item.UserId == user.Id);
        if (profile is not null)
        {
            return profile;
        }

        profile = CreateDefaultSocialProfile(user);
        _socialProfiles.Add(profile);
        PersistSnapshot();
        return profile;
    }

    private static SocialProfile CreateDefaultSocialProfile(UserAccount user)
        => new()
        {
            UserId = user.Id,
            DisplayName = user.Name,
            Bio = "Perfil social pronto para partilhar viagens, custos e rotas.",
            HomeCity = string.Empty,
            DrivingStyle = "Equilibrado",
            PreferredFuelBrands = string.Empty,
            IsOpenToCarpool = true,
            ShareLiveTripStatus = true
        };

    private IEnumerable<DirectMessage> MessagesForUser(UserAccount user)
        => _messages.Where(message =>
            message.OwnerId == user.Id
            || message.SenderUserId == user.Id
            || message.RecipientEmail.Contains(user.Email, StringComparison.OrdinalIgnoreCase));

    private int CountMessagesForAdmin(UserAccount user)
        => MessagesForUser(user).Count();

    private IEnumerable<CollaborativeTrip> CollaborativeTripsForUser(UserAccount user)
        => _collaborativeTrips.Where(trip =>
            trip.OwnerId == user.Id
            || trip.Members.Contains(user.Email, StringComparer.OrdinalIgnoreCase));

    private static bool IsLocked(UserAccount user)
        => user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow;

    private static string BuildAccessModeLabel(UserAccount user)
    {
        var providers = user.LinkedProviders
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .DefaultIfEmpty(user.AuthProvider)
            .Where(provider => !string.IsNullOrWhiteSpace(provider))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var hasLocal = providers.Any(provider => provider.Equals("Local", StringComparison.OrdinalIgnoreCase));
        var hasExternal = providers.Any(provider => !provider.Equals("Local", StringComparison.OrdinalIgnoreCase));

        return (hasLocal, hasExternal) switch
        {
            (true, true) => "Local + SSO",
            (false, true) => "SSO",
            _ => "Local"
        };
    }

    private static string BuildThreadKey(string leftEmail, string rightEmail)
        => string.Join("|", new[] { leftEmail.Trim().ToLowerInvariant(), rightEmail.Trim().ToLowerInvariant() }.Order(StringComparer.Ordinal));

    private AuthenticatorSetupViewModel BuildAuthenticatorSetup(UserAccount user, bool includeQrCode)
    {
        var uri = string.IsNullOrWhiteSpace(user.AuthenticatorKey)
            ? string.Empty
            : _authenticatorService.BuildOtpAuthUri(user.Email, user.AuthenticatorKey);

        return new AuthenticatorSetupViewModel
        {
            IsEnabled = user.TwoFactorEnabled,
            SharedKey = string.IsNullOrWhiteSpace(user.AuthenticatorKey)
                ? string.Empty
                : AuthenticatorService.FormatSecretForDisplay(user.AuthenticatorKey),
            AuthenticatorUri = uri,
            QrCodeDataUri = includeQrCode && !string.IsNullOrWhiteSpace(uri)
                ? _authenticatorService.BuildQrCodeDataUri(uri)
                : string.Empty,
            RecoveryCodeCount = ReadRecoveryCodeHashes(user).Count
        };
    }

    private static List<string> ReadRecoveryCodeHashes(UserAccount user)
    {
        if (string.IsNullOrWhiteSpace(user.RecoveryCodesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(user.RecoveryCodesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string EnsureProvider(string currentProviders, string provider)
    {
        var providers = currentProviders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (providers.All(item => !item.Equals(provider, StringComparison.OrdinalIgnoreCase)))
        {
            providers.Add(provider);
        }

        return string.Join(", ", providers.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static double capacityForVehicle(VehicleProfile vehicle)
        => vehicle.FuelKind == FuelKind.Electric
            ? vehicle.BatteryCapacity ?? 0
            : vehicle.TankCapacity;

    private void AddVehicleActivity(
        Guid ownerId,
        VehicleProfile vehicle,
        VehicleActivityType type,
        string title,
        string details,
        double? amount = null,
        string unit = "",
        double? costEstimate = null,
        double? unitPrice = null,
        double? odometerKm = null,
        int? levelPercentAfter = null,
        Guid? relatedRouteId = null)
    {
        _vehicleActivities.Insert(0, new VehicleActivity
        {
            OwnerId = ownerId,
            VehicleId = vehicle.Id,
            RelatedRouteId = relatedRouteId,
            Type = type,
            Title = title,
            Details = details,
            Amount = amount,
            Unit = unit,
            CostEstimate = costEstimate,
            UnitPrice = unitPrice,
            OdometerKm = odometerKm,
            LevelPercentAfter = levelPercentAfter
        });
    }

    private static string BuildFuelEntryDetails(VehicleProfile vehicle, string? stationName, double amount, double? unitPrice, bool fillToFull)
    {
        var unit = vehicle.FuelKind == FuelKind.Electric ? "kWh" : "L";
        var action = fillToFull
            ? (vehicle.FuelKind == FuelKind.Electric ? "Bateria marcada como cheia" : "Depósito marcado como cheio")
            : vehicle.FuelKind == FuelKind.Electric ? "Carregamento registado" : "Abastecimento registado";
        var stationText = string.IsNullOrWhiteSpace(stationName) ? string.Empty : $" em {stationName.Trim()}";
        var priceText = unitPrice.HasValue ? $" a {unitPrice.Value:0.000} EUR/{unit}" : string.Empty;
        return $"{action}{stationText}: {amount:0.0} {unit}{priceText}.";
    }
}
