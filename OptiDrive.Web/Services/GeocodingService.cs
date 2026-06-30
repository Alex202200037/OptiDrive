using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class GeocodingService(HttpClient httpClient, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GeocodedPlace?> GeocodeAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = await SearchAsync(query, 1, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<GeocodedPlace>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var googleKey = configuration["GoogleMaps:ApiKey"];
        if (!string.IsNullOrWhiteSpace(googleKey))
        {
            var googleResults = await SearchWithGoogleAsync(query, limit, googleKey, cancellationToken);
            if (googleResults.Count > 0)
            {
                return googleResults;
            }
        }

        return await SearchWithNominatimAsync(query, limit, cancellationToken);
    }

    private async Task<IReadOnlyList<GeocodedPlace>> SearchWithGoogleAsync(string query, int limit, string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var url = QueryHelpers.AddQueryString(
                "https://maps.googleapis.com/maps/api/geocode/json",
                new Dictionary<string, string?>
                {
                    ["address"] = query,
                    ["region"] = "pt",
                    ["language"] = "pt-PT",
                    ["key"] = apiKey
                });

            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<GoogleGeocodingResponse>(content, JsonOptions, cancellationToken);
            return (payload?.Results ?? [])
                .Take(limit)
                .Select(result => new GeocodedPlace
                {
                    Query = query,
                    DisplayName = result.FormattedAddress,
                    Latitude = result.Geometry?.Location?.Lat ?? 0,
                    Longitude = result.Geometry?.Location?.Lng ?? 0,
                    Provider = "Google Geocoding"
                })
                .Where(place => place.Latitude != 0 || place.Longitude != 0)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private async Task<IReadOnlyList<GeocodedPlace>> SearchWithNominatimAsync(string query, int limit, CancellationToken cancellationToken)
    {
        var url = QueryHelpers.AddQueryString(
            "https://nominatim.openstreetmap.org/search",
            new Dictionary<string, string?>
            {
                ["format"] = "jsonv2",
                ["limit"] = limit.ToString(CultureInfo.InvariantCulture),
                ["countrycodes"] = "pt",
                ["q"] = query
            });

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", "OptiDrive/1.0");
        request.Headers.TryAddWithoutValidation("Accept-Language", "pt-PT,pt;q=0.9,en;q=0.7");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<List<NominatimSearchItem>>(content, JsonOptions, cancellationToken) ?? [];
        return payload
            .Take(limit)
            .Select(item => new GeocodedPlace
            {
                Query = query,
                DisplayName = item.DisplayName,
                Latitude = double.TryParse(item.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) ? lat : 0,
                Longitude = double.TryParse(item.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng) ? lng : 0,
                Provider = "Nominatim"
            })
            .Where(place => place.Latitude != 0 || place.Longitude != 0)
            .ToList();
    }

    private sealed class GoogleGeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GoogleGeocodingResult> Results { get; set; } = [];
    }

    private sealed class GoogleGeocodingResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("geometry")]
        public GoogleGeometry? Geometry { get; set; }
    }

    private sealed class GoogleGeometry
    {
        [JsonPropertyName("location")]
        public GoogleLocation? Location { get; set; }
    }

    private sealed class GoogleLocation
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}
