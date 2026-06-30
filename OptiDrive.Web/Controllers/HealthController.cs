using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(AppStateService appState, IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var googleMapsConfigured = !string.IsNullOrWhiteSpace(configuration["GoogleMaps:ApiKey"]);
        var openChargeMapConfigured = !string.IsNullOrWhiteSpace(configuration["OpenChargeMap:ApiKey"]);
        var googleOAuthConfigured = !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"])
                                    && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);
        var microsoftOAuthConfigured = !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientId"])
                                       && !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientSecret"]);
        var appleOAuthConfigured = IsAppleAuthConfigured();

        return Ok(new
        {
            status = "Healthy",
            product = "OptiDrive",
            environment = environment.EnvironmentName,
            checkedAtUtc = DateTime.UtcNow,
            counts = new
            {
                users = appState.UserCount(),
                vehicles = appState.VehicleCount(),
                routes = appState.RouteCount()
            },
            integrations = new
            {
                googleMaps = googleMapsConfigured,
                openChargeMap = openChargeMapConfigured,
                googleOAuth = googleOAuthConfigured,
                microsoftOAuth = microsoftOAuthConfigured,
                appleOAuth = appleOAuthConfigured,
                authenticatorMfa = true,
                socialPulse = true
            }
        });
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
