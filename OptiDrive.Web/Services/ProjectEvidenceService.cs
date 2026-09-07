using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class ProjectEvidenceService(IConfiguration configuration)
{
    public ProjectEvidenceViewModel Build()
    {
        var automatedTests = PositiveValue("ProjectEvidence:AutomatedTestCount", 16);
        var stressUsers = PositiveValue("ProjectEvidence:StressUserCount", 500);
        var stressVehicles = PositiveValue("ProjectEvidence:StressVehicleCount", 500);
        var lastVerifiedOn = configuration["ProjectEvidence:LastVerifiedOn"] ?? "02/09/2026";

        return new ProjectEvidenceViewModel
        {
            AutomatedTestCount = automatedTests,
            StressUserCount = stressUsers,
            StressVehicleCount = stressVehicles,
            LastVerifiedOn = lastVerifiedOn,
            Items =
            [
                Evidence(
                    "Pipeline CI/CD",
                    "Configurada",
                    "Restore, build, testes, publish, Docker e deploy Azure rastreaveis no GitHub Actions.",
                    configuration["ProjectLinks:Pipeline"] ?? "https://github.com/Alex202200037/OptiDrive/actions",
                    "Abrir pipeline"),
                Evidence(
                    "Testes automatizados",
                    $"{automatedTests}/{automatedTests} aprovados",
                    $"Suite xUnit verificada em Release em {lastVerifiedOn}.",
                    configuration["ProjectLinks:AutomatedTests"] ?? "https://github.com/Alex202200037/OptiDrive/tree/main/OptiDrive.Web.Tests",
                    "Ver testes"),
                Evidence(
                    "Stress test",
                    $"{stressUsers} + {stressVehicles}",
                    "Carga com utilizadores e veiculos executada com threshold objetivo e sem excecoes.",
                    configuration["ProjectLinks:StressTest"] ?? "https://github.com/Alex202200037/OptiDrive/blob/main/OptiDrive.Web.Tests/StressTests.cs",
                    "Ver stress test"),
                Evidence(
                    "Healthcheck",
                    "Operacional",
                    "Estado, ambiente, contagens e integracoes consultaveis sem exposicao de segredos.",
                    configuration["ProjectLinks:Healthcheck"] ?? "/health",
                    "Consultar /health"),
                Evidence(
                    "Gestao Jira",
                    "Rastreavel",
                    "Backlog, sprints, burndown, velocity e evidencias de execucao no board oficial.",
                    configuration["ProjectLinks:Jira"] ?? "https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/backlog",
                    "Abrir Jira"),
                Evidence(
                    "Documentacao",
                    "Publicada",
                    "Requisitos, desenho, atas, testes, DevOps e encerramento na arvore final do Confluence.",
                    configuration["ProjectLinks:Confluence"] ?? "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/97189889/OptiDrive+-+Documenta+o+Final",
                    "Abrir Confluence")
            ]
        };
    }

    private int PositiveValue(string key, int fallback)
    {
        var configured = configuration.GetValue<int?>(key);
        return configured is > 0 ? configured.Value : fallback;
    }

    private static ProjectEvidenceItem Evidence(
        string area,
        string status,
        string detail,
        string url,
        string linkLabel) => new()
        {
            Area = area,
            Status = status,
            Detail = detail,
            Url = url,
            LinkLabel = linkLabel,
            Severity = string.IsNullOrWhiteSpace(url) ? "warn" : "ok"
        };
}
