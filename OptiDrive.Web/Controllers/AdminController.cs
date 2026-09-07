using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class AdminController(
    AppStateService appState,
    ExternalFuelStationService fuelStations,
    ProjectEvidenceService projectEvidence,
    IConfiguration configuration,
    IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (CurrentAdmin() is null)
        {
            return RedirectForNonAdmin();
        }

        return View(new AdminViewModel
        {
            UserCount = appState.UserCount(),
            VehicleCount = appState.VehicleCount(),
            RouteCount = appState.RouteCount(),
            ActiveUserCount = appState.ActiveUserCount(),
            LockedUserCount = appState.LockedUserCount(),
            TwoFactorUserCount = appState.TwoFactorUserCount(),
            PendingSocialRequestCount = appState.PendingSocialRequestCount(),
            ActiveTripCount = appState.ActiveTripCount(),
            Users = appState.BuildAdminUserSummaries(),
            RecentRoutes = appState.BuildAdminRouteSummaries(),
            Trips = appState.BuildAdminTripSummaries(),
            Stations = appState.AllStations(),
            Reports = appState.AllReports().Take(8).ToList(),
            SyncStatuses = appState.SyncStatuses(),
            Readiness = BuildReadiness(),
            ProjectEvidence = projectEvidence.Build()
        });
    }

    [HttpPost]
    public async Task<IActionResult> RefreshStations(CancellationToken cancellationToken)
    {
        if (CurrentAdmin() is null) return RedirectForNonAdmin();
        await fuelStations.GetAllStationsAsync(true, cancellationToken);
        TempData["AdminSuccess"] = "Postos atualizados e estado de sincronizacao revisto.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SetUserRole(Guid targetUserId, UserRole role)
    {
        var admin = CurrentAdmin();
        if (admin is null) return RedirectForNonAdmin();
        SetAdminFlash(appState.TrySetUserRole(admin.Id, targetUserId, role, out var message), message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult LockUser(Guid targetUserId)
    {
        var admin = CurrentAdmin();
        if (admin is null) return RedirectForNonAdmin();
        SetAdminFlash(appState.TrySetUserLock(admin.Id, targetUserId, true, out var message), message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult UnlockUser(Guid targetUserId)
    {
        var admin = CurrentAdmin();
        if (admin is null) return RedirectForNonAdmin();
        SetAdminFlash(appState.TrySetUserLock(admin.Id, targetUserId, false, out var message), message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ConfirmUserEmail(Guid targetUserId)
    {
        var admin = CurrentAdmin();
        if (admin is null) return RedirectForNonAdmin();
        SetAdminFlash(appState.TryConfirmUserEmail(admin.Id, targetUserId, out var message), message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ResetUserMfa(Guid targetUserId)
    {
        var admin = CurrentAdmin();
        if (admin is null) return RedirectForNonAdmin();
        SetAdminFlash(appState.TryResetUserMfa(admin.Id, targetUserId, out var message), message);
        return RedirectToAction(nameof(Index));
    }

    private UserAccount? CurrentAdmin()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null)
        {
            return null;
        }

        var user = appState.GetUser(userId.Value);
        return user?.Role == UserRole.Admin ? user : null;
    }

    private IActionResult RedirectForNonAdmin()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        return userId is null
            ? RedirectToAction("Login", "Home")
            : RedirectToAction("Index", "Dashboard");
    }

    private void SetAdminFlash(bool success, string message)
    {
        TempData[success ? "AdminSuccess" : "AdminWarning"] = message;
    }

    private IReadOnlyList<SystemReadinessItem> BuildReadiness()
    {
        var googleMapsConfigured = !string.IsNullOrWhiteSpace(configuration["GoogleMaps:ApiKey"]);
        var openChargeConfigured = !string.IsNullOrWhiteSpace(configuration["OpenChargeMap:ApiKey"]);
        var googleOAuthConfigured = !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"])
                                    && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);
        var microsoftOAuthConfigured = !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientId"])
                                       && !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientSecret"]);
        var appleOAuthConfigured = IsAppleAuthConfigured();

        return
        [
            new SystemReadinessItem
            {
                Area = "Runtime",
                Status = environment.EnvironmentName,
                Detail = "Aplicacao .NET 8 MVC operacional em ambiente web e Docker.",
                Severity = "ok"
            },
            new SystemReadinessItem
            {
                Area = "Base de dados",
                Status = "Operacional",
                Detail = "SQLite persistente com bootstrap de schema e dados iniciais.",
                Severity = "ok"
            },
            new SystemReadinessItem
            {
                Area = "Google Maps",
                Status = googleMapsConfigured ? "Configurado" : "Por associar",
                Detail = googleMapsConfigured ? "Mapas, geocoding e directions preparados." : "Fornecedor externo preparado para ativacao.",
                Severity = googleMapsConfigured ? "ok" : "warn"
            },
            new SystemReadinessItem
            {
                Area = "OpenChargeMap",
                Status = openChargeConfigured ? "Configurado" : "Por associar",
                Detail = openChargeConfigured ? "Carregadores eletricos integrados." : "Fornecedor externo preparado para ativacao.",
                Severity = openChargeConfigured ? "ok" : "warn"
            },
            new SystemReadinessItem
            {
                Area = "Google OAuth",
                Status = googleOAuthConfigured ? "Ativo" : "Por associar",
                Detail = googleOAuthConfigured ? "Login Google pronto." : "Fornecedor de identidade preparado para ativacao.",
                Severity = googleOAuthConfigured ? "ok" : "warn"
            },
            new SystemReadinessItem
            {
                Area = "Microsoft OAuth",
                Status = microsoftOAuthConfigured ? "Ativo" : "Por associar",
                Detail = microsoftOAuthConfigured ? "Login Microsoft pronto." : "Fornecedor de identidade preparado para ativacao.",
                Severity = microsoftOAuthConfigured ? "ok" : "warn"
            },
            new SystemReadinessItem
            {
                Area = "Apple OAuth",
                Status = appleOAuthConfigured ? "Ativo" : "Por associar",
                Detail = appleOAuthConfigured ? "Login Apple pronto." : "Fornecedor de identidade preparado para ativacao.",
                Severity = appleOAuthConfigured ? "ok" : "warn"
            },
            new SystemReadinessItem
            {
                Area = "MFA Authenticator",
                Status = "Operacional",
                Detail = "TOTP, QR code e codigos de recuperacao implementados.",
                Severity = "ok"
            },
            new SystemReadinessItem
            {
                Area = "Social online",
                Status = "Operacional",
                Detail = "Mensagens, comunidade, viagens colaborativas e live pulse.",
                Severity = "ok"
            }
        ];
    }

    private bool IsAppleAuthConfigured()
    {
        var clientId = configuration["Authentication:Apple:ClientId"];
        var clientSecret = configuration["Authentication:Apple:ClientSecret"];
        var teamId = configuration["Authentication:Apple:TeamId"];
        var keyId = configuration["Authentication:Apple:KeyId"];
        var configuredPath = configuration["Authentication:Apple:PrivateKeyPath"];
        var privateKeyPath = string.IsNullOrWhiteSpace(configuredPath)
            ? null
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);

        return !string.IsNullOrWhiteSpace(clientId)
               && (!string.IsNullOrWhiteSpace(clientSecret)
                   || (!string.IsNullOrWhiteSpace(teamId)
                       && !string.IsNullOrWhiteSpace(keyId)
                       && !string.IsNullOrWhiteSpace(privateKeyPath)
                       && System.IO.File.Exists(privateKeyPath)));
    }
}
