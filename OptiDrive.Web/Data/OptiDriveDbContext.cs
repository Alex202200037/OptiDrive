using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Data;

public sealed class OptiDriveDbContext(DbContextOptions<OptiDriveDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ValueComparer<List<string>> StringListComparer = new(
        (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>(), StringComparer.OrdinalIgnoreCase),
        value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(item))),
        value => value.ToList());

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<SocialProfile> SocialProfiles => Set<SocialProfile>();
    public DbSet<VehicleProfile> Vehicles => Set<VehicleProfile>();
    public DbSet<FuelStation> Stations => Set<FuelStation>();
    public DbSet<PlannedRoute> Routes => Set<PlannedRoute>();
    public DbSet<VehicleActivity> VehicleActivities => Set<VehicleActivity>();
    public DbSet<TrustedContact> Contacts => Set<TrustedContact>();
    public DbSet<SocialConnectionRequest> SocialConnectionRequests => Set<SocialConnectionRequest>();
    public DbSet<SharedVehicle> SharedVehicles => Set<SharedVehicle>();
    public DbSet<TravelGroup> TravelGroups => Set<TravelGroup>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<CollaborativeTrip> CollaborativeTrips => Set<CollaborativeTrip>();
    public DbSet<PriceReport> PriceReports => Set<PriceReport>();
    public DbSet<ApiSyncStatus> SyncStatuses => Set<ApiSyncStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>().HasKey(entity => entity.Id);
        modelBuilder.Entity<UserAccount>().HasIndex(entity => entity.Email).IsUnique();

        modelBuilder.Entity<SocialProfile>().HasKey(entity => entity.Id);
        modelBuilder.Entity<SocialProfile>().HasIndex(entity => entity.UserId).IsUnique();

        modelBuilder.Entity<VehicleProfile>().HasKey(entity => entity.Id);
        modelBuilder.Entity<FuelStation>().HasKey(entity => entity.Id);
        modelBuilder.Entity<PlannedRoute>().HasKey(entity => entity.Id);
        modelBuilder.Entity<VehicleActivity>().HasKey(entity => entity.Id);
        modelBuilder.Entity<TrustedContact>().HasKey(entity => entity.Id);
        modelBuilder.Entity<SocialConnectionRequest>().HasKey(entity => entity.Id);
        modelBuilder.Entity<SocialConnectionRequest>()
            .HasIndex(entity => new { entity.RequesterId, entity.TargetUserId });
        modelBuilder.Entity<SharedVehicle>().HasKey(entity => entity.Id);
        modelBuilder.Entity<TravelGroup>().HasKey(entity => entity.Id);
        modelBuilder.Entity<DirectMessage>().HasKey(entity => entity.Id);
        modelBuilder.Entity<CollaborativeTrip>().HasKey(entity => entity.Id);
        modelBuilder.Entity<PriceReport>().HasKey(entity => entity.Id);
        modelBuilder.Entity<ApiSyncStatus>().HasKey(entity => entity.SourceName);

        modelBuilder.Entity<TravelGroup>()
            .Property(entity => entity.Members)
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonOptions),
                value => JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer);

        modelBuilder.Entity<CollaborativeTrip>()
            .Property(entity => entity.Members)
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonOptions),
                value => JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer);
    }
}
