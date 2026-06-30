using System.Security.Claims;
using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class HomeController(AppStateService appState, IConfiguration configuration, IWebHostEnvironment environment) : Controller
{
    private const string ExternalAuthenticationScheme = "OptiDrive.External";

    [HttpGet]
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is not null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpGet]
    public IActionResult Login(string? externalError = null)
    {
        if (!string.IsNullOrWhiteSpace(externalError))
        {
            TempData["Error"] = externalError;
        }

        SetExternalAuthFlags();
        return View(new LoginInputModel());
    }

    [HttpPost]
    public IActionResult Login(LoginInputModel input)
    {
        if (!ModelState.IsValid)
        {
            SetExternalAuthFlags();
            return View(input);
        }

        var user = appState.ValidateUser(input.Email, input.Password);
        if (user is null)
        {
            TempData["Error"] = "Credenciais invalidas.";
            SetExternalAuthFlags();
            return View(input);
        }

        if (user.TwoFactorEnabled)
        {
            HttpContext.Session.SetPendingMfaUserId(user.Id);
            return RedirectToAction(nameof(TwoFactor));
        }

        HttpContext.Session.SetCurrentUserId(user.Id);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult TwoFactor()
    {
        if (HttpContext.Session.GetPendingMfaUserId() is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new TwoFactorInputModel());
    }

    [HttpPost]
    public IActionResult TwoFactor(TwoFactorInputModel input)
    {
        var pendingUserId = HttpContext.Session.GetPendingMfaUserId();
        if (pendingUserId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            SetExternalAuthFlags();
            return View(input);
        }

        if (!appState.VerifySecondFactor(pendingUserId.Value, input.Code))
        {
            TempData["Error"] = "Codigo invalido. Usa o Microsoft Authenticator/Google Authenticator ou um codigo de recuperacao.";
            return View(input);
        }

        HttpContext.Session.ClearPendingMfaUser();
        HttpContext.Session.SetCurrentUserId(pendingUserId.Value);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public IActionResult LoginWithGoogle()
    {
        if (!IsGoogleAuthEnabled())
        {
            TempData["Error"] = "Google login ainda nao tem ClientId/ClientSecret configurados.";
            return RedirectToAction(nameof(Login));
        }

        var normalizedStart = RedirectToProviderLocalStartIfNeeded(nameof(LoginWithGoogleStart), "Google");
        if (normalizedStart is not null)
        {
            return normalizedStart;
        }

        return StartExternalLogin("Google", GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult LoginWithGoogleStart()
    {
        if (!IsGoogleAuthEnabled())
        {
            TempData["Error"] = "Google login ainda nao tem ClientId/ClientSecret configurados.";
            return RedirectToAction(nameof(Login));
        }

        return StartExternalLogin("Google", GoogleDefaults.AuthenticationScheme);
    }

    [HttpPost]
    public IActionResult LoginWithMicrosoft()
    {
        if (!IsMicrosoftAuthEnabled())
        {
            TempData["Error"] = "Microsoft login ainda nao tem ClientId/ClientSecret configurados.";
            return RedirectToAction(nameof(Login));
        }

        var normalizedStart = RedirectToProviderLocalStartIfNeeded(nameof(LoginWithMicrosoftStart), "Microsoft");
        if (normalizedStart is not null)
        {
            return normalizedStart;
        }

        return StartExternalLogin("Microsoft", MicrosoftAccountDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult LoginWithMicrosoftStart()
    {
        if (!IsMicrosoftAuthEnabled())
        {
            TempData["Error"] = "Microsoft login ainda nao tem ClientId/ClientSecret configurados.";
            return RedirectToAction(nameof(Login));
        }

        return StartExternalLogin("Microsoft", MicrosoftAccountDefaults.AuthenticationScheme);
    }

    private IActionResult StartExternalLogin(string provider, string authenticationScheme)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalCallback), new { provider })
        };

        return Challenge(properties, authenticationScheme);
    }

    [HttpPost]
    public IActionResult LoginWithApple()
    {
        if (!IsAppleAuthEnabled())
        {
            return RedirectToAction(nameof(AppleUnavailable));
        }

        var normalizedStart = RedirectToProviderLocalStartIfNeeded(nameof(LoginWithAppleStart), "Apple");
        if (normalizedStart is not null)
        {
            return normalizedStart;
        }

        return StartExternalLogin("Apple", AppleAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult LoginWithAppleStart()
    {
        if (!IsAppleAuthEnabled())
        {
            return RedirectToAction(nameof(AppleUnavailable));
        }

        return StartExternalLogin("Apple", AppleAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult AppleUnavailable() => View();

    [HttpGet]
    public Task<IActionResult> GoogleCallback() => ExternalCallback("Google");

    [HttpGet]
    public async Task<IActionResult> ExternalCallback(string provider)
    {
        var result = await HttpContext.AuthenticateAsync(ExternalAuthenticationScheme);
        if (!result.Succeeded || result.Principal is null)
        {
            TempData["Error"] = $"Nao foi possivel autenticar com {provider}.";
            return RedirectToAction(nameof(Login));
        }

        var email = FindExternalEmail(result.Principal);
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = $"A conta {provider} nao devolveu email.";
            await HttpContext.SignOutAsync(ExternalAuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];
        var externalId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? email;
        var avatar = result.Principal.FindFirstValue("picture") ?? string.Empty;
        var normalizedProvider = NormalizeProvider(provider);
        var user = appState.FindOrCreateExternalUser(name, email, normalizedProvider, externalId, avatar);
        if (user.TwoFactorEnabled)
        {
            HttpContext.Session.SetPendingMfaUserId(user.Id);
            await HttpContext.SignOutAsync(ExternalAuthenticationScheme);
            return RedirectToAction(nameof(TwoFactor));
        }

        HttpContext.Session.SetCurrentUserId(user.Id);
        await HttpContext.SignOutAsync(ExternalAuthenticationScheme);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Error()
        => View(new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier
        });

    [HttpGet]
    public IActionResult Register()
    {
        SetExternalAuthFlags();
        return View(new RegisterInputModel());
    }

    [HttpPost]
    public IActionResult Register(RegisterInputModel input)
    {
        if (!ModelState.IsValid)
        {
            SetExternalAuthFlags();
            return View(input);
        }

        try
        {
            var user = appState.Register(input.Name, input.Email, input.Password);
            HttpContext.Session.SetCurrentUserId(user.Id);
            TempData["Success"] = "Conta criada. Ativa o Authenticator para proteger o acesso.";
            return RedirectToAction("Index", "Profile");
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            SetExternalAuthFlags();
            return View(input);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.ClearCurrentUser();
        HttpContext.Session.ClearPendingMfaUser();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

    private void SetExternalAuthFlags()
    {
        ViewBag.GoogleAuthEnabled = IsGoogleAuthEnabled();
        ViewBag.MicrosoftAuthEnabled = IsMicrosoftAuthEnabled();
        ViewBag.AppleAuthEnabled = IsAppleAuthEnabled();
        ViewBag.AppleSignInVisible = true;
    }

    private string NormalizeProvider(string provider)
    {
        if (provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase)) return "Microsoft";
        if (provider.Equals("Apple", StringComparison.OrdinalIgnoreCase)) return "Apple";
        return "Google";
    }

    private string? ResolveApplePrivateKeyPath(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath)) return null;
        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }

    private static string? FindExternalEmail(ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email)
           ?? principal.FindFirstValue("email")
           ?? principal.FindFirstValue("preferred_username")
           ?? principal.FindFirstValue("upn")
           ?? principal.FindFirstValue("unique_name");

    private IActionResult? RedirectToProviderLocalStartIfNeeded(string actionName, string provider)
    {
        if (!IsLoopbackHost(Request.Host.Host))
        {
            return null;
        }

        var actionPath = Url.Action(actionName, "Home") ?? $"/Home/{actionName}";
        var targetUrl = BuildProviderLocalActionUrl(provider, actionPath);
        var targetUri = new Uri(targetUrl);

        var currentPort = Request.Host.Port ?? (Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80);
        var targetPort = targetUri.IsDefaultPort ? (targetUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80) : targetUri.Port;
        if (Request.Scheme.Equals(targetUri.Scheme, StringComparison.OrdinalIgnoreCase)
            && Request.Host.Host.Equals(targetUri.Host, StringComparison.OrdinalIgnoreCase)
            && currentPort == targetPort)
        {
            return null;
        }

        return Redirect(targetUrl);
    }

    private string BuildProviderLocalActionUrl(string provider, string actionPath)
    {
        var configuredBaseUrl = configuration[$"Authentication:{provider}:LocalBaseUrl"]
                                ?? configuration["Authentication:OAuthLocalBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl)
            && Uri.TryCreate(configuredBaseUrl.TrimEnd('/'), UriKind.Absolute, out var configuredBaseUri))
        {
            return new Uri(configuredBaseUri, actionPath.TrimStart('/')).ToString();
        }

        var port = Request.Host.Port is null ? string.Empty : $":{Request.Host.Port}";
        var host = "localhost";
        return $"{Request.Scheme}://{host}{port}{actionPath}";
    }

    private static bool IsLoopbackHost(string host)
        => host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
           || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
           || host.Equals("::1", StringComparison.OrdinalIgnoreCase)
           || host.Equals("[::1]", StringComparison.OrdinalIgnoreCase);
}
