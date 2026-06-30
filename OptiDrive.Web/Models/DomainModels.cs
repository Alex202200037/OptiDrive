namespace OptiDrive.Web.Models;

public enum UserRole
{
    User,
    Admin
}

public enum FuelKind
{
    Gasoline95,
    Gasoline98,
    Diesel,
    GPL,
    Electric
}

public enum TollClass
{
    Class1,
    Class2,
    Class3,
    Class4
}

public sealed class UserAccount
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string AuthProvider { get; set; } = "Local";
    public string ExternalProviderId { get; set; } = string.Empty;
    public string LinkedProviders { get; set; } = "Local";
    public string AvatarUrl { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string AuthenticatorKey { get; set; } = string.Empty;
    public string RecoveryCodesJson { get; set; } = "[]";
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
}

public sealed class SocialProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string HomeCity { get; set; } = string.Empty;
    public string DrivingStyle { get; set; } = "Equilibrado";
    public string PreferredFuelBrands { get; set; } = string.Empty;
    public bool IsOpenToCarpool { get; set; } = true;
    public bool ShareLiveTripStatus { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class VehicleProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string RegistrationPlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public FuelKind FuelKind { get; set; }
    public TollClass TollClass { get; set; }
    public double AverageConsumption { get; set; }
    public double TankCapacity { get; set; }
    public double? BatteryCapacity { get; set; }
    public double AverageRangeKm { get; set; }
    public int CurrentLevelPercent { get; set; }
    public double OdometerKm { get; set; }
    public double? NextServiceKm { get; set; }
    public bool IsDefault { get; set; }
    public DateOnly? LastMaintenance { get; set; }
    public int TireHealthPercent { get; set; } = 82;
    public DateOnly? InspectionDueDate { get; set; }
    public DateOnly? InsuranceDueDate { get; set; }
    public DateOnly? TaxDueDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class FuelStation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public FuelKind FuelKind { get; set; }
    public double Price { get; set; }
    public bool IsLowCost { get; set; }
    public bool IsOpen24h { get; set; }
    public string Services { get; set; } = string.Empty;
    public string AvailableFuels { get; set; } = string.Empty;
    public string AvailableFuelKinds { get; set; } = string.Empty;
    public string FuelPriceSummary { get; set; } = string.Empty;
    public string FuelPriceMapJson { get; set; } = "{}";
    public bool IsElectricCharging { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "Platform";
}

public sealed class PlannedRoute
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid VehicleId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Waypoints { get; set; } = string.Empty;
    public bool AvoidTolls { get; set; }
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    public string RoutePathJson { get; set; } = "[]";
    public double DistanceKm { get; set; }
    public int EtaMinutes { get; set; }
    public double FastestCost { get; set; }
    public double CheapestCost { get; set; }
    public double TollCost { get; set; }
    public double CarbonKg { get; set; }
    public double EstimatedConsumptionAmount { get; set; }
    public int AverageSpeedKmh { get; set; }
    public double AdjustedConsumptionPer100 { get; set; }
    public double ConsumptionSpeedFactor { get; set; } = 1;
    public int MinimumArrivalLevelPercent { get; set; }
    public double TotalSuggestedReplenishmentAmount { get; set; }
    public int EstimatedArrivalLevelPercent { get; set; }
    public int EstimatedDirectArrivalLevelPercent { get; set; }
    public int SuggestedStopCount { get; set; }
    public bool RequiresIntermediateStops { get; set; }
    public int SavingsScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string CriticalAlert { get; set; } = string.Empty;
    public string RecommendedStationName { get; set; } = string.Empty;
    public double RecommendedChargeAmount { get; set; }
    public string SuggestedStopsJson { get; set; } = "[]";
    public bool IsAppliedToVehicle { get; set; }
    public DateTime? AppliedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class RouteStopPlan
{
    public string StationName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public FuelKind FuelKind { get; set; }
    public double Price { get; set; }
    public double ProgressKm { get; set; }
    public double SuggestedAmount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public enum VehicleActivityType
{
    Created,
    Updated,
    Refuel,
    Charge,
    TripApplied
}

public sealed class VehicleActivity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? RelatedRouteId { get; set; }
    public VehicleActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public double? Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? CostEstimate { get; set; }
    public double? UnitPrice { get; set; }
    public double? OdometerKm { get; set; }
    public int? LevelPercentAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class TrustedContact
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public enum SocialConnectionStatus
{
    Pending,
    Accepted,
    Declined
}

public sealed class SocialConnectionRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid RequesterId { get; set; }
    public Guid TargetUserId { get; set; }
    public SocialConnectionStatus Status { get; set; } = SocialConnectionStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAtUtc { get; set; }
}

public sealed class SharedVehicle
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid VehicleId { get; set; }
    public string TargetEmail { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
}

public sealed class TravelGroup
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SplitAmount { get; set; }
    public List<string> Members { get; set; } = [];
}

public enum CollaborativeTripStatus
{
    Planned,
    Scheduled,
    Active,
    Completed,
    Cancelled
}

public sealed class DirectMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string ThreadKey { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? RelatedTripId { get; set; }
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class CollaborativeTrip
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid? RouteId { get; set; }
    public Guid? VehicleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string MeetingPoint { get; set; } = string.Empty;
    public DateTime? StartsAtUtc { get; set; }
    public CollaborativeTripStatus Status { get; set; } = CollaborativeTripStatus.Planned;
    public List<string> Members { get; set; } = [];
    public decimal EstimatedSplitAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public sealed class PriceReport
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid StationId { get; set; }
    public Guid UserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public decimal ReportedPrice { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ApiSyncStatus
{
    public string SourceName { get; set; } = string.Empty;
    public DateTime? LastSuccessUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public bool IsSuccess { get; set; }
    public int ItemCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class GeocodedPlace
{
    public string Query { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Provider { get; set; } = string.Empty;
}
