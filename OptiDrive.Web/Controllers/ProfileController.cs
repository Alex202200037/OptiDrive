using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class ProfileController(
    AppStateService appState,
    ExternalVehicleCatalogService vehicleCatalog,
    ExternalFuelStationService fuelStations,
    IConfiguration configuration,
    IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        await vehicleCatalog.GetMakesAsync(cancellationToken);
        await fuelStations.GetAllStationsAsync(false, cancellationToken);

        var model = appState.BuildDashboard(userId.Value);
        model.AuthenticatorSetup = appState.GetAuthenticatorSetup(userId.Value);
        model.GoogleAuthEnabled = IsGoogleAuthEnabled();
        model.MicrosoftAuthEnabled = IsMicrosoftAuthEnabled();
        model.AppleAuthEnabled = IsAppleAuthEnabled();
        return View(model);
    }

    [HttpPost]
    public IActionResult EnableAuthenticator(TwoFactorInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        input.Code = ResolveSubmittedAuthenticatorCode(input.Code);
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            ModelState.Remove(nameof(TwoFactorInputModel.Code));
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Introduz o codigo de 6 digitos da app Authenticator.";
            return RedirectToAction(nameof(Index));
        }

        if (!appState.EnableAuthenticator(userId.Value, input.Code, out var recoveryCodes))
        {
            TempData["Error"] = "Codigo invalido. Confirma se digitalizaste o QR code correto.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Autenticacao em dois passos ativada. Guarda os codigos de recuperacao.";
        TempData["RecoveryCodes"] = string.Join(";", recoveryCodes);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult RegenerateRecoveryCodes()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        var recoveryCodes = appState.RegenerateRecoveryCodes(userId.Value);
        TempData["Success"] = "Novos codigos de recuperacao gerados.";
        TempData["RecoveryCodes"] = string.Join(";", recoveryCodes);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ResetAuthenticator()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        appState.ResetAuthenticator(userId.Value);
        TempData["Success"] = "Authenticator reiniciado. Digitaliza o novo QR code para voltar a ativar.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult DisableAuthenticator()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        appState.DisableAuthenticator(userId.Value);
        TempData["Success"] = "Autenticacao em dois passos desativada.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsGoogleAuthEnabled()
        => !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"])
           && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);

    private bool IsMicrosoftAuthEnabled()
        => !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientId"])
           && !string.IsNullOrWhiteSpace(configuration["Authentication:Microsoft:ClientSecret"]);

    private bool IsAppleAuthEnabled()
    {
        var clientId = configuration["Authentication:Apple:ClientId"];
        var clientSecret = configuration["Authentication:Apple:ClientSecret"];
        var teamId = configuration["Authentication:Apple:TeamId"];
        var keyId = configuration["Authentication:Apple:KeyId"];
        var privateKeyPath = ResolveApplePrivateKeyPath(configuration["Authentication:Apple:PrivateKeyPath"]);

        return !string.IsNullOrWhiteSpace(clientId)
               && (!string.IsNullOrWhiteSpace(clientSecret)
                   || (!string.IsNullOrWhiteSpace(teamId)
                       && !string.IsNullOrWhiteSpace(keyId)
                       && !string.IsNullOrWhiteSpace(privateKeyPath)
                       && System.IO.File.Exists(privateKeyPath)));
    }

    private string? ResolveApplePrivateKeyPath(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath)) return null;
        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }

    private string ResolveSubmittedAuthenticatorCode(string? code)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        if (!Request.HasFormContentType)
        {
            return string.Empty;
        }

        return Request.Form[nameof(TwoFactorInputModel.Code)].FirstOrDefault()
               ?? Request.Form["TwoFactorInput.Code"].FirstOrDefault()
               ?? string.Empty;
    }
}
