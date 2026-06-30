using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class ExternalFuelStationService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    AppStateService appState,
    ILogger<ExternalFuelStationService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string FuelTypesCacheKey = "dgeg-fuel-types";
    private const string StationsCacheKey = "dgeg-stations-all";

    public async Task<IReadOnlyList<DgegFuelTypeItem>> GetFuelTypesAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(FuelTypesCacheKey, out IReadOnlyList<DgegFuelTypeItem>? cached) && cached is not null)
        {
            return cached;
        }

        using var response = await httpClient.GetAsync("https://precoscombustiveis.dgeg.gov.pt/api/PrecoComb/GetTiposCombustiveis", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<DgegResponse<DgegFuelTypeItem>>(content, JsonOptions, cancellationToken);
        var items = payload?.Result
            .Where(item => item.ViewWebsite && item.Active && item.RoadVehicle)
            .OrderBy(item => item.Description)
            .ToList() ?? [];

        memoryCache.Set(FuelTypesCacheKey, items, TimeSpan.FromHours(12));
        return items;
    }

    public async Task<IReadOnlyList<FuelStation>> GetAllStationsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && memoryCache.TryGetValue(StationsCacheKey, out IReadOnlyList<FuelStation>? cached) && cached is not null)
        {
            return cached;
        }

        appState.StartSyncAttempt("FuelStations");

        try
        {
            var fuelTypes = await GetFuelTypesAsync(cancellationToken);
            var fuelTypeIds = string.Join(",", fuelTypes.Select(item => item.Id));
            var rawStations = new List<DgegStationItem>();
            var page = 1;
            var total = int.MaxValue;

            while (rawStations.Count < total)
            {
                var url = $"https://precoscombustiveis.dgeg.gov.pt/api/PrecoComb/PesquisarPostos?idsTiposComb={Uri.EscapeDataString(fuelTypeIds)}&qtdPorPagina=9999&pagina={page}";
                using var response = await httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                var payload = await JsonSerializer.DeserializeAsync<DgegResponse<DgegStationItem>>(content, JsonOptions, cancellationToken);
                var chunk = payload?.Result ?? [];
                if (chunk.Count == 0)
                {
                    break;
                }

                total = chunk[0].TotalQuantity <= 0 ? chunk.Count : chunk[0].TotalQuantity;
                rawStations.AddRange(chunk);
                page += 1;

                if (chunk.Count < 9999)
                {
                    break;
                }
            }

            var mapped = rawStations
                .GroupBy(BuildGroupKey)
                .Select(MapStationGroup)
                .Where(item => item is not null)
                .Cast<FuelStation>()
                .OrderBy(station => station.Price <= 0 ? double.MaxValue : station.Price)
                .ThenBy(station => station.Name)
                .ToList();

            memoryCache.Set(StationsCacheKey, mapped, TimeSpan.FromMinutes(30));
            appState.ReplaceStations(mapped, "DGEG");
            appState.CompleteSyncAttempt("FuelStations", true, mapped.Count, "Postos sincronizados da DGEG com combustiveis agregados por posto.");
            return mapped;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao sincronizar postos DGEG.");
            appState.CompleteSyncAttempt("FuelStations", false, appState.AllStations().Count, $"Falha ao sincronizar postos: {exception.Message}");
            return appState.AllStations();
        }
    }

    public async Task WarmAsync(CancellationToken cancellationToken = default)
    {
        _ = await GetFuelTypesAsync(cancellationToken);
        _ = await GetAllStationsAsync(false, cancellationToken);
    }

    private static string BuildGroupKey(DgegStationItem source)
    {
        if (source.Id > 0)
        {
            return source.Id.ToString(CultureInfo.InvariantCulture);
        }

        return string.Join("|", new[]
        {
            (source.Name ?? string.Empty).Trim().ToUpperInvariant(),
            (source.Brand ?? string.Empty).Trim().ToUpperInvariant(),
            (source.Municipality ?? string.Empty).Trim().ToUpperInvariant(),
            (source.Address ?? string.Empty).Trim().ToUpperInvariant(),
            source.Latitude.ToString("0.000000", CultureInfo.InvariantCulture),
            source.Longitude.ToString("0.000000", CultureInfo.InvariantCulture)
        });
    }

    private static FuelStation? MapStationGroup(IGrouping<string, DgegStationItem> group)
    {
        var first = group.First();
        var mappedFuels = group
            .Select(source => new
            {
                Source = source,
                Kind = TryMapFuelKind(source.Fuel, out var fuelKind) ? fuelKind : (FuelKind?)null,
                Price = TryParsePrice(source.Price, out var price) ? price : (double?)null
            })
            .Where(item => item.Kind.HasValue)
            .ToList();

        if (mappedFuels.Count == 0)
        {
            return null;
        }

        var orderedFuelKinds = mappedFuels
            .Select(item => item.Kind!.Value)
            .Distinct()
            .OrderBy(GetFuelSortOrder)
            .ToList();

        var priceMap = orderedFuelKinds.ToDictionary(
            kind => kind.ToString(),
            kind => mappedFuels
                .Where(item => item.Kind == kind && item.Price.HasValue)
                .Select(item => item.Price!.Value)
                .DefaultIfEmpty(0)
                .Min());

        var primaryFuel = orderedFuelKinds.First();
        var primaryPrice = priceMap[primaryFuel.ToString()];

        var priceSummary = orderedFuelKinds
            .Select(kind =>
            {
                var bestPrice = mappedFuels
                    .Where(item => item.Kind == kind && item.Price.HasValue)
                    .Select(item => item.Price!.Value)
                    .DefaultIfEmpty(0)
                    .Min();

                return bestPrice > 0
                    ? $"{FuelLabel(kind)} {bestPrice:0.000} EUR"
                    : FuelLabel(kind);
            })
            .ToList();

        var rawFuelNames = group
            .Select(item => (item.Fuel ?? string.Empty).Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        var lastUpdated = group
            .Select(item => ParseUpdatedAt(item.UpdatedAt))
            .OrderByDescending(item => item)
            .FirstOrDefault(DateTime.UtcNow);

        return new FuelStation
        {
            ExternalId = first.Id > 0 ? $"DGEG-{first.Id}" : BuildGroupKey(first),
            Name = (first.Name ?? string.Empty).Trim(),
            Brand = string.IsNullOrWhiteSpace(first.Brand) ? "Sem marca" : first.Brand.Trim(),
            City = (first.Municipality ?? string.Empty).Trim(),
            District = (first.District ?? string.Empty).Trim(),
            Address = (first.Address ?? string.Empty).Trim(),
            Latitude = first.Latitude,
            Longitude = first.Longitude,
            FuelKind = primaryFuel,
            Price = primaryPrice,
            IsLowCost = IsLowCostBrand(first.Brand),
            IsOpen24h = false,
            Services = string.Join(" · ", new[] { (first.StationType ?? string.Empty).Trim(), (first.Locality ?? string.Empty).Trim() }.Where(value => !string.IsNullOrWhiteSpace(value))),
            AvailableFuels = rawFuelNames.Count == 0 ? FuelLabel(primaryFuel) : string.Join(", ", rawFuelNames),
            AvailableFuelKinds = string.Join('|', orderedFuelKinds.Select(kind => kind.ToString())),
            FuelPriceSummary = string.Join(" · ", priceSummary),
            FuelPriceMapJson = JsonSerializer.Serialize(priceMap, JsonOptions),
            IsElectricCharging = false,
            LastUpdatedAt = lastUpdated,
            Source = "DGEG"
        };
    }

    private static DateTime ParseUpdatedAt(string raw)
    {
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate)
            ? parsedDate.ToUniversalTime()
            : DateTime.UtcNow;
    }

    private static bool TryParsePrice(string raw, out double price)
    {
        var normalized = raw
            .Replace("€", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(",", ".", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
    }

    private static bool TryMapFuelKind(string rawFuel, out FuelKind fuelKind)
    {
        rawFuel ??= string.Empty;

        if (rawFuel.Contains("Gasóleo", StringComparison.OrdinalIgnoreCase) ||
            rawFuel.Contains("Gasoleo", StringComparison.OrdinalIgnoreCase))
        {
            fuelKind = FuelKind.Diesel;
            return true;
        }

        if (rawFuel.Contains("GPL", StringComparison.OrdinalIgnoreCase))
        {
            fuelKind = FuelKind.GPL;
            return true;
        }

        if (rawFuel.Contains("98", StringComparison.OrdinalIgnoreCase))
        {
            fuelKind = FuelKind.Gasoline98;
            return true;
        }

        if (rawFuel.Contains("95", StringComparison.OrdinalIgnoreCase))
        {
            fuelKind = FuelKind.Gasoline95;
            return true;
        }

        fuelKind = default;
        return false;
    }

    private static int GetFuelSortOrder(FuelKind fuelKind)
        => fuelKind switch
        {
            FuelKind.Diesel => 0,
            FuelKind.Gasoline95 => 1,
            FuelKind.Gasoline98 => 2,
            FuelKind.GPL => 3,
            FuelKind.Electric => 4,
            _ => 99
        };

    private static string FuelLabel(FuelKind fuelKind)
        => fuelKind switch
        {
            FuelKind.Gasoline95 => "Gasolina 95",
            FuelKind.Gasoline98 => "Gasolina 98",
            FuelKind.Diesel => "Gasoleo",
            FuelKind.GPL => "GPL",
            FuelKind.Electric => "Eletrico",
            _ => fuelKind.ToString()
        };

    private static bool IsLowCostBrand(string brand)
    {
        var normalized = (brand ?? string.Empty).Trim().ToUpperInvariant();
        return normalized.Contains("PRIO") ||
               normalized.Contains("AUCHAN") ||
               normalized.Contains("INTERMARCH") ||
               normalized.Contains("BXPRESS") ||
               normalized.Contains("PLENERGY") ||
               normalized.Contains("PETROPRIX") ||
               normalized.Contains("CONTINENTE");
    }
}
