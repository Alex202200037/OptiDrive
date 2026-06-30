using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class ExternalVehicleCatalogService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    AppStateService appState)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string MakesCacheKey = "vehicle-catalog-makes";

    public async Task<IReadOnlyList<string>> GetMakesAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(MakesCacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        var status = appState.StartSyncAttempt("VehicleCatalog");
        try
        {
            using var response = await httpClient.GetAsync("https://vpic.nhtsa.dot.gov/api/vehicles/GetMakesForVehicleType/car?format=json", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<VpicResponse<VpicMakeItem>>(content, JsonOptions, cancellationToken);
            var makes = payload?.Results
                .Select(item => item.MakeName.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList() ?? [];

            memoryCache.Set(MakesCacheKey, makes, TimeSpan.FromHours(24));
            appState.CompleteSyncAttempt("VehicleCatalog", true, makes.Count, "Marcas carregadas da vPIC.");
            return makes;
        }
        catch (Exception exception)
        {
            appState.CompleteSyncAttempt("VehicleCatalog", false, 0, $"Falha ao obter marcas: {exception.Message}");
            return [];
        }
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(string make, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return [];
        }

        var cacheKey = $"vehicle-models::{make.Trim().ToUpperInvariant()}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            using var response = await httpClient.GetAsync(
                $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMake/{Uri.EscapeDataString(make)}?format=json",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<VpicResponse<VpicModelItem>>(content, JsonOptions, cancellationToken);
            var models = payload?.Results
                .Select(item => item.ModelName.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList() ?? [];

            memoryCache.Set(cacheKey, models, TimeSpan.FromHours(24));
            return models;
        }
        catch
        {
            return [];
        }
    }
}
