using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class ExternalElectricStationService(
    HttpClient httpClient,
    IConfiguration configuration,
    IMemoryCache memoryCache)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string CacheKey = "openchargemap-stations-pt";

    public async Task<IReadOnlyList<FuelStation>> GetElectricStationsAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(CacheKey, out IReadOnlyList<FuelStation>? cached) && cached is not null)
        {
            return cached;
        }

        var apiKey = configuration["OpenChargeMap:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildFallbackStations();
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.openchargemap.io/v3/poi/?output=json&countrycode=PT&maxresults=500&compact=true&verbose=false");
            request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            request.Headers.TryAddWithoutValidation("User-Agent", "OptiDrive/1.0");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<List<OpenChargeMapItem>>(content, JsonOptions, cancellationToken) ?? [];
            var mapped = payload
                .Where(item => item.AddressInfo is not null)
                .Select(MapStation)
                .Where(item => item is not null)
                .Cast<FuelStation>()
                .ToList();

            memoryCache.Set(CacheKey, mapped, TimeSpan.FromHours(1));
            return mapped;
        }
        catch
        {
            return BuildFallbackStations();
        }
    }

    private static IReadOnlyList<FuelStation> BuildFallbackStations()
    {
        return
        [
            new FuelStation
            {
                ExternalId = "SEED-EV-IONITY-GRANDOLA",
                Name = "Ionity Grandola",
                Brand = "Ionity",
                City = "Grandola",
                District = "Setubal",
                Address = "A2, area de servico Grandola",
                Latitude = 38.1840,
                Longitude = -8.5660,
                FuelKind = FuelKind.Electric,
                Price = 0.49,
                Services = "HPC, WC",
                AvailableFuels = "Carregamento eletrico",
                AvailableFuelKinds = "Electric",
                FuelPriceSummary = "Eletricidade 0.490 EUR/kWh",
                FuelPriceMapJson = "{\"Electric\":0.49}",
                IsElectricCharging = true,
                IsOpen24h = true,
                Source = "Fallback EV"
            },
            new FuelStation
            {
                ExternalId = "SEED-EV-CONTINENTE-SETUBAL",
                Name = "Continente Charge Setubal",
                Brand = "Continente",
                City = "Setubal",
                District = "Setubal",
                Address = "Zona comercial Setubal",
                Latitude = 38.5320,
                Longitude = -8.8890,
                FuelKind = FuelKind.Electric,
                Price = 0.42,
                Services = "FastCharge, Loja, Cafe",
                AvailableFuels = "Carregamento eletrico",
                AvailableFuelKinds = "Electric",
                FuelPriceSummary = "Eletricidade 0.420 EUR/kWh",
                FuelPriceMapJson = "{\"Electric\":0.42}",
                IsElectricCharging = true,
                IsOpen24h = true,
                Source = "Fallback EV"
            }
        ];
    }

    private static FuelStation? MapStation(OpenChargeMapItem source)
    {
        var address = source.AddressInfo;
        if (address is null)
        {
            return null;
        }

        var connectors = source.Connections?
            .Select(connection => connection.ConnectionType?.Title)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        var power = source.Connections?
            .Where(connection => connection.PowerKw.HasValue)
            .Select(connection => connection.PowerKw!.Value)
            .DefaultIfEmpty()
            .Max();

        return new FuelStation
        {
            ExternalId = $"OCM-{source.Id}",
            Name = string.IsNullOrWhiteSpace(address.Title) ? "Posto elétrico" : address.Title!,
            Brand = string.IsNullOrWhiteSpace(source.OperatorInfo?.Title) ? "Rede elétrica" : source.OperatorInfo!.Title!,
            City = address.Town ?? string.Empty,
            District = address.StateOrProvince ?? string.Empty,
            Address = address.AddressLine1 ?? string.Empty,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            FuelKind = FuelKind.Electric,
            Price = TryParseUsageCost(source.UsageCost),
            IsLowCost = false,
            IsOpen24h = true,
            Services = connectors.Count == 0 ? "Carregamento elétrico" : string.Join(", ", connectors),
            AvailableFuels = connectors.Count == 0 ? "Carregamento elétrico" : string.Join(", ", connectors),
            AvailableFuelKinds = FuelKind.Electric.ToString(),
            FuelPriceSummary = power > 0
                ? $"Eletricidade ({power:0.#} kW)"
                : "Eletricidade",
            FuelPriceMapJson = JsonSerializer.Serialize(new Dictionary<string, double> { [FuelKind.Electric.ToString()] = TryParseUsageCost(source.UsageCost) }, JsonOptions),
            IsElectricCharging = true,
            Source = "Open Charge Map"
        };
    }

    private static double TryParseUsageCost(string? usageCost)
    {
        if (string.IsNullOrWhiteSpace(usageCost))
        {
            return 0;
        }

        var normalized = new string(usageCost
            .Where(character => char.IsDigit(character) || character is ',' or '.')
            .ToArray())
            .Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }
}
