using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using OptiDrive.Web.Controllers;
using OptiDrive.Web.Services;
using Xunit;

namespace OptiDrive.Web.Tests;

public sealed class HealthControllerTests
{
    [Fact]
    public void Index_ReturnsHealthyStatusWithoutSecrets()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleMaps:ApiKey"] = "configured-for-test",
                ["Authentication:Google:ClientId"] = "client",
                ["Authentication:Google:ClientSecret"] = "secret-value"
            })
            .Build();
        var controller = new HealthController(
            new AppStateService(),
            configuration,
            new TestWebHostEnvironment());

        var result = Assert.IsType<OkObjectResult>(controller.Index());
        var json = JsonSerializer.Serialize(result.Value);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("OptiDrive", document.RootElement.GetProperty("product").GetString());
        Assert.True(document.RootElement.GetProperty("counts").GetProperty("users").GetInt32() >= 3);
        Assert.True(document.RootElement.GetProperty("integrations").GetProperty("googleMaps").GetBoolean());
        Assert.DoesNotContain("secret-value", json, StringComparison.Ordinal);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "OptiDrive.Web.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
