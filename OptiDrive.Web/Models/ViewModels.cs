using System.ComponentModel.DataAnnotations;

namespace OptiDrive.Web.Models;

public sealed class LoginInputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class TwoFactorInputModel
{
    [Required]
    [Display(Name = "Codigo do Authenticator")]
    public string Code { get; set; } = string.Empty;
}

public sealed class RegisterInputModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public sealed class VehicleInputModel
{
    [Required]
    public string Nickname { get; set; } = string.Empty;

    public string? RegistrationPlate { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; } = 2022;

    public FuelKind FuelKind { get; set; } = FuelKind.Diesel;
    public TollClass TollClass { get; set; } = TollClass.Class1;
    public double AverageConsumption { get; set; } = 5.6;
    public double TankCapacity { get; set; } = 50;
    public double? BatteryCapacity { get; set; }
    public double AverageRangeKm { get; set; } = 850;
    public int CurrentLevelPercent { get; set; } = 75;
    public double OdometerKm { get; set; }
    public double? NextServiceKm { get; set; }
    public DateOnly? LastMaintenance { get; set; }
    [Range(0, 100)]
    public int TireHealthPercent { get; set; } = 82;
    public DateOnly? InspectionDueDate { get; set; }
    public DateOnly? InsuranceDueDate { get; set; }
    public DateOnly? TaxDueDate { get; set; }
    public string? Notes { get; set; }
    public bool UseCatalogSelection { get; set; } = true;
}

public sealed class VehicleStatusInputModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Nickname { get; set; } = string.Empty;

    public string? RegistrationPlate { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; } = 2022;

    public FuelKind FuelKind { get; set; }
    public TollClass TollClass { get; set; }

    [Range(0, 100)]
    public int CurrentLevelPercent { get; set; }

    [Range(0.1, 100)]
    public double AverageConsumption { get; set; }

    [Range(1, 5000)]
    public double AverageRangeKm { get; set; }

    [Range(0, 1000)]
    public double TankCapacity { get; set; }

    [Range(0, 1000)]
    public double? BatteryCapacity { get; set; }

    [Range(0, 1000000)]
    public double OdometerKm { get; set; }

    [Range(0, 1000000)]
    public double? NextServiceKm { get; set; }

    public DateOnly? LastMaintenance { get; set; }

    [Range(0, 100)]
    public int TireHealthPercent { get; set; }

    public DateOnly? InspectionDueDate { get; set; }
    public DateOnly? InsuranceDueDate { get; set; }
    public DateOnly? TaxDueDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class RefuelVehicleInputModel
{
    [Required]
    public Guid Id { get; set; }

    [Range(0, 1000)]
    public double Amount { get; set; }

    [Range(0, 20)]
    public double? UnitPrice { get; set; }

    public string? StationName { get; set; }

    public bool FillToFull { get; set; }
}

public sealed class RoutePlanInputModel
{
    [Required]
    public Guid VehicleId { get; set; }

    [Required]
    public string Origin { get; set; } = "Setubal";

    [Required]
    public string Destination { get; set; } = "Faro";

    public string? Waypoints { get; set; }

    [Range(60, 140)]
    public int AverageSpeed { get; set; } = 110;

    public bool AvoidTolls { get; set; }
}

public sealed class ContactInputModel
{
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public Guid? TargetUserId { get; set; }
}

public sealed class ShareVehicleInputModel
{
    [Required]
    public Guid VehicleId { get; set; }

    [EmailAddress]
    public string TargetEmail { get; set; } = string.Empty;

    public Guid? TargetUserId { get; set; }

    public bool CanEdit { get; set; }
}

public sealed class GroupInputModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public decimal SplitAmount { get; set; }
    public string MemberEmails { get; set; } = string.Empty;
}

public sealed class PriceReportInputModel
{
    [Required]
    public Guid StationId { get; set; }

    [Range(0.1, 10)]
    public decimal ReportedPrice { get; set; }

    public string Note { get; set; } = string.Empty;
}

public sealed class SocialProfileInputModel
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;
    public string HomeCity { get; set; } = string.Empty;
    public string DrivingStyle { get; set; } = "Equilibrado";
    public string PreferredFuelBrands { get; set; } = string.Empty;
    public bool IsOpenToCarpool { get; set; } = true;
    public bool ShareLiveTripStatus { get; set; } = true;
}

public sealed class DirectMessageInputModel
{
    [EmailAddress]
    public string RecipientEmail { get; set; } = string.Empty;

    public Guid? RecipientUserId { get; set; }

    [Required]
    [MinLength(2)]
    public string Body { get; set; } = string.Empty;

    public Guid? RelatedTripId { get; set; }
}

public sealed class CollaborativeTripInputModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid? RouteId { get; set; }
    public Guid? VehicleId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string MeetingPoint { get; set; } = string.Empty;
    public DateTime? StartsAtLocal { get; set; }
    public string MemberEmails { get; set; } = string.Empty;
    public List<Guid> MemberUserIds { get; set; } = [];
    public decimal EstimatedSplitAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class SocialMemberCardViewModel
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string HomeCity { get; set; } = string.Empty;
    public string DrivingStyle { get; set; } = string.Empty;
    public string PreferredFuelBrands { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsOpenToCarpool { get; set; }
    public bool ShareLiveTripStatus { get; set; }
    public int VehicleCount { get; set; }
    public int SharedTripCount { get; set; }
    public string AccessState { get; set; } = "None";
    public Guid? RequestId { get; set; }
}

public sealed class DashboardViewModel
{
    public UserAccount User { get; set; } = new();
    public SocialProfile SocialProfile { get; set; } = new();
    public IReadOnlyList<VehicleProfile> Vehicles { get; set; } = [];
    public IReadOnlyList<FuelStation> Stations { get; set; } = [];
    public IReadOnlyList<PlannedRoute> Routes { get; set; } = [];
    public IReadOnlyList<TrustedContact> Contacts { get; set; } = [];
    public IReadOnlyList<SharedVehicle> SharedVehicles { get; set; } = [];
    public IReadOnlyList<TravelGroup> Groups { get; set; } = [];
    public IReadOnlyList<PriceReport> Reports { get; set; } = [];
    public IReadOnlyList<string> VehicleMakes { get; set; } = [];
    public IReadOnlyDictionary<string, ApiSyncStatus> SyncStatuses { get; set; } = new Dictionary<string, ApiSyncStatus>();
    public IReadOnlyList<RouteStopPlan> LatestRouteStops { get; set; } = [];
    public IReadOnlyList<VehicleActivity> VehicleActivities { get; set; } = [];
    public IReadOnlyList<DirectMessage> DirectMessages { get; set; } = [];
    public IReadOnlyList<CollaborativeTrip> CollaborativeTrips { get; set; } = [];
    public IReadOnlyList<UserAccount> SocialMembers { get; set; } = [];
    public IReadOnlyList<SocialMemberCardViewModel> SocialDirectory { get; set; } = [];
    public IReadOnlyList<SocialMemberCardViewModel> IncomingAccessRequests { get; set; } = [];
    public Guid? SelectedVehicleId { get; set; }
    public Guid? SelectedRouteId { get; set; }
    public VehicleInputModel VehicleInput { get; set; } = new();
    public RoutePlanInputModel RouteInput { get; set; } = new();
    public ContactInputModel ContactInput { get; set; } = new();
    public ShareVehicleInputModel ShareInput { get; set; } = new();
    public GroupInputModel GroupInput { get; set; } = new();
    public PriceReportInputModel PriceReportInput { get; set; } = new();
    public SocialProfileInputModel SocialProfileInput { get; set; } = new();
    public DirectMessageInputModel MessageInput { get; set; } = new();
    public CollaborativeTripInputModel CollaborativeTripInput { get; set; } = new();
    public AuthenticatorSetupViewModel AuthenticatorSetup { get; set; } = new();
    public TwoFactorInputModel TwoFactorInput { get; set; } = new();
    public bool GoogleAuthEnabled { get; set; }
    public bool MicrosoftAuthEnabled { get; set; }
    public bool AppleAuthEnabled { get; set; }
}

public sealed class AuthenticatorSetupViewModel
{
    public bool IsEnabled { get; set; }
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    public string QrCodeDataUri { get; set; } = string.Empty;
    public IReadOnlyList<string> NewRecoveryCodes { get; set; } = [];
    public int RecoveryCodeCount { get; set; }
}

public sealed class AdminViewModel
{
    public int UserCount { get; set; }
    public int VehicleCount { get; set; }
    public int RouteCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int LockedUserCount { get; set; }
    public int TwoFactorUserCount { get; set; }
    public int PendingSocialRequestCount { get; set; }
    public int ActiveTripCount { get; set; }
    public IReadOnlyList<AdminUserSummaryViewModel> Users { get; set; } = [];
    public IReadOnlyList<AdminRouteSummaryViewModel> RecentRoutes { get; set; } = [];
    public IReadOnlyList<AdminTripSummaryViewModel> Trips { get; set; } = [];
    public IReadOnlyList<FuelStation> Stations { get; set; } = [];
    public IReadOnlyList<PriceReport> Reports { get; set; } = [];
    public IReadOnlyDictionary<string, ApiSyncStatus> SyncStatuses { get; set; } = new Dictionary<string, ApiSyncStatus>();
    public IReadOnlyList<SystemReadinessItem> Readiness { get; set; } = [];
}

public sealed class AdminUserSummaryViewModel
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsLocked { get; set; }
    public string AccessMode { get; set; } = "Local";
    public int FailedLoginAttempts { get; set; }
    public int VehicleCount { get; set; }
    public int RouteCount { get; set; }
    public int MessageCount { get; set; }
    public int TripCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}

public sealed class AdminRouteSummaryViewModel
{
    public Guid RouteId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double CheapestCost { get; set; }
    public int EstimatedArrivalLevelPercent { get; set; }
    public bool RequiresIntermediateStops { get; set; }
    public bool IsAppliedToVehicle { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AdminTripSummaryViewModel
{
    public Guid TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public CollaborativeTripStatus Status { get; set; }
    public int MemberCount { get; set; }
    public decimal EstimatedSplitAmount { get; set; }
    public DateTime? StartsAtUtc { get; set; }
}

public sealed class SystemReadinessItem
{
    public string Area { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Severity { get; set; } = "ok";
}
