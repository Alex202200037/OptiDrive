using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OptiDrive.Web.Data;
using OptiDrive.Web.Services;

var builder = WebApplication.CreateBuilder(args);
const string ExternalAuthenticationScheme = "OptiDrive.External";
LoadDotEnv(builder.Environment.ContentRootPath);
builder.Configuration.AddEnvironmentVariables();

var defaultDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(defaultDataDirectory);
var defaultConnectionString = $"Data Source={Path.Combine(defaultDataDirectory, "optidrive.db")}";
var connectionString = builder.Configuration.GetConnectionString("OptiDrive") ?? defaultConnectionString;
var runtimeDataDirectory = ResolveSqliteDataDirectory(builder.Environment.ContentRootPath, connectionString) ?? defaultDataDirectory;
Directory.CreateDirectory(runtimeDataDirectory);
var dataProtectionDirectory = Path.Combine(runtimeDataDirectory, "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionDirectory);
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
var appleClientId = builder.Configuration["Authentication:Apple:ClientId"];
var appleClientSecret = builder.Configuration["Authentication:Apple:ClientSecret"];
var appleTeamId = builder.Configuration["Authentication:Apple:TeamId"];
var appleKeyId = builder.Configuration["Authentication:Apple:KeyId"];
var applePrivateKeyPath = builder.Configuration["Authentication:Apple:PrivateKeyPath"];
var resolvedApplePrivateKeyPath = string.IsNullOrWhiteSpace(applePrivateKeyPath)
    ? null
    : Path.IsPathRooted(applePrivateKeyPath)
        ? applePrivateKeyPath
        : Path.Combine(builder.Environment.ContentRootPath, applePrivateKeyPath);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "OptiDrive.Session.v2";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionDirectory))
    .SetApplicationName("OptiDrive");
builder.Services.AddMemoryCache();
builder.Services.AddDbContextFactory<OptiDriveDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<AuthenticatorService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddScoped<SmartSaveService>();
builder.Services.AddHttpClient<ExternalVehicleCatalogService>();
builder.Services.AddHttpClient<ExternalFuelStationService>();
builder.Services.AddHttpClient<ExternalElectricStationService>();
builder.Services.AddHttpClient<GeocodingService>();
builder.Services.AddHttpClient<GoogleDirectionsService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "OptiDrive.Auth.v2";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddCookie(ExternalAuthenticationScheme, options =>
    {
        options.Cookie.Name = "OptiDrive.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddGoogle(options =>
    {
        options.ClientId = string.IsNullOrWhiteSpace(googleClientId) ? "not-configured" : googleClientId;
        options.ClientSecret = string.IsNullOrWhiteSpace(googleClientSecret) ? "not-configured" : googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SignInScheme = ExternalAuthenticationScheme;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ClaimActions.MapJsonKey("picture", "picture", "url");
        options.Events = BuildOAuthEvents("Google");
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = string.IsNullOrWhiteSpace(microsoftClientId) ? "not-configured" : microsoftClientId;
        options.ClientSecret = string.IsNullOrWhiteSpace(microsoftClientSecret) ? "not-configured" : microsoftClientSecret;
        options.CallbackPath = "/signin-microsoft";
        options.SignInScheme = ExternalAuthenticationScheme;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SaveTokens = true;
        options.Events = BuildOAuthEvents("Microsoft");
    })
    .AddApple(options =>
    {
        options.ClientId = string.IsNullOrWhiteSpace(appleClientId) ? "not-configured" : appleClientId;
        options.ClientSecret = string.IsNullOrWhiteSpace(appleClientSecret) ? "not-configured" : appleClientSecret;
        options.CallbackPath = "/signin-apple";
        options.SignInScheme = ExternalAuthenticationScheme;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events = BuildAppleAuthenticationEvents("Apple");

        if (!string.IsNullOrWhiteSpace(appleClientId)
            && string.IsNullOrWhiteSpace(appleClientSecret)
            && !string.IsNullOrWhiteSpace(appleTeamId)
            && !string.IsNullOrWhiteSpace(appleKeyId)
            && !string.IsNullOrWhiteSpace(resolvedApplePrivateKeyPath))
        {
            options.TeamId = appleTeamId;
            options.KeyId = appleKeyId;
            options.GenerateClientSecret = true;
            options.UsePrivateKey(_ => new PhysicalFileInfo(new FileInfo(resolvedApplePrivateKeyPath)));
        }
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<OptiDriveDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await DatabaseBootstrapper.EnsureRuntimeSchemaAsync(db);
    _ = scope.ServiceProvider.GetRequiredService<AppStateService>();
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void LoadDotEnv(string contentRootPath)
{
    var candidates = new[]
    {
        Path.Combine(contentRootPath, ".env"),
        Path.Combine(Directory.GetParent(contentRootPath)?.FullName ?? contentRootPath, ".env")
    };

    foreach (var envPath in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        if (!File.Exists(envPath)) continue;

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0) continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(key)) continue;

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            var aspNetKey = ToAspNetConfigurationKey(key);
            if (!string.IsNullOrWhiteSpace(aspNetKey) && Environment.GetEnvironmentVariable(aspNetKey) is null)
            {
                Environment.SetEnvironmentVariable(aspNetKey, value);
            }
        }
    }
}

static string? ToAspNetConfigurationKey(string key)
    => key switch
    {
        "GOOGLE_MAPS_API_KEY" => "GoogleMaps__ApiKey",
        "OPEN_CHARGE_MAP_API_KEY" => "OpenChargeMap__ApiKey",
        "GOOGLE_OAUTH_CLIENT_ID" => "Authentication__Google__ClientId",
        "GOOGLE_OAUTH_CLIENT_SECRET" => "Authentication__Google__ClientSecret",
        "MICROSOFT_OAUTH_CLIENT_ID" => "Authentication__Microsoft__ClientId",
        "MICROSOFT_OAUTH_CLIENT_SECRET" => "Authentication__Microsoft__ClientSecret",
        "APPLE_OAUTH_CLIENT_ID" => "Authentication__Apple__ClientId",
        "APPLE_OAUTH_CLIENT_SECRET" => "Authentication__Apple__ClientSecret",
        "APPLE_OAUTH_TEAM_ID" => "Authentication__Apple__TeamId",
        "APPLE_OAUTH_KEY_ID" => "Authentication__Apple__KeyId",
        "APPLE_OAUTH_PRIVATE_KEY_PATH" => "Authentication__Apple__PrivateKeyPath",
        _ => null
    };

static OAuthEvents BuildOAuthEvents(string provider)
    => new()
    {
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            var message = Uri.EscapeDataString(
                $"Nao foi possivel concluir o login com {provider}. Confirma se estas a usar http://localhost e se o Redirect URI esta configurado.");
            context.Response.Redirect($"/Home/Login?externalError={message}");
            return Task.CompletedTask;
        }
    };

static AppleAuthenticationEvents BuildAppleAuthenticationEvents(string provider)
    => new()
    {
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            var message = Uri.EscapeDataString(
                $"Nao foi possivel concluir o login com {provider}. Confirma se estas a usar http://localhost e se o Redirect URI esta configurado.");
            context.Response.Redirect($"/Home/Login?externalError={message}");
            return Task.CompletedTask;
        }
    };

static string? ResolveSqliteDataDirectory(string contentRootPath, string connectionString)
{
    const string marker = "Data Source=";
    var dataSource = connectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .FirstOrDefault(part => part.StartsWith(marker, StringComparison.OrdinalIgnoreCase));

    if (dataSource is null)
    {
        return null;
    }

    var path = dataSource[marker.Length..].Trim().Trim('"');
    if (string.IsNullOrWhiteSpace(path) || path.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    var resolvedPath = Path.IsPathRooted(path) ? path : Path.Combine(contentRootPath, path);
    return Path.GetDirectoryName(resolvedPath);
}
