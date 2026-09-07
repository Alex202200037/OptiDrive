using Microsoft.Extensions.Configuration;
using OptiDrive.Web.Services;
using Xunit;

namespace OptiDrive.Web.Tests;

public sealed class ProjectEvidenceServiceTests
{
    [Fact]
    public void Build_ReturnsAllActionableEvidenceSources()
    {
        var evidence = BuildService([]).Build();

        Assert.Equal(6, evidence.Items.Count);
        Assert.All(evidence.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Area));
            Assert.False(string.IsNullOrWhiteSpace(item.Status));
            Assert.False(string.IsNullOrWhiteSpace(item.Detail));
            Assert.False(string.IsNullOrWhiteSpace(item.Url));
            Assert.False(string.IsNullOrWhiteSpace(item.LinkLabel));
            Assert.Equal("ok", item.Severity);
        });
    }

    [Fact]
    public void Build_UsesConfiguredQualityMetrics()
    {
        var service = BuildService(new Dictionary<string, string?>
        {
            ["ProjectEvidence:AutomatedTestCount"] = "16",
            ["ProjectEvidence:StressUserCount"] = "650",
            ["ProjectEvidence:StressVehicleCount"] = "525",
            ["ProjectEvidence:LastVerifiedOn"] = "08/09/2026"
        });

        var evidence = service.Build();

        Assert.Equal(16, evidence.AutomatedTestCount);
        Assert.Equal(650, evidence.StressUserCount);
        Assert.Equal(525, evidence.StressVehicleCount);
        Assert.Equal("08/09/2026", evidence.LastVerifiedOn);
        Assert.Contains(evidence.Items, item => item.Status == "16/16 aprovados");
        Assert.Contains(evidence.Items, item => item.Status == "650 + 525");
    }

    [Fact]
    public void Build_FallsBackWhenMetricsAreInvalid()
    {
        var service = BuildService(new Dictionary<string, string?>
        {
            ["ProjectEvidence:AutomatedTestCount"] = "0",
            ["ProjectEvidence:StressUserCount"] = "-1",
            ["ProjectEvidence:StressVehicleCount"] = "0"
        });

        var evidence = service.Build();

        Assert.Equal(16, evidence.AutomatedTestCount);
        Assert.Equal(500, evidence.StressUserCount);
        Assert.Equal(500, evidence.StressVehicleCount);
    }

    private static ProjectEvidenceService BuildService(IEnumerable<KeyValuePair<string, string?>> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new ProjectEvidenceService(configuration);
    }
}
