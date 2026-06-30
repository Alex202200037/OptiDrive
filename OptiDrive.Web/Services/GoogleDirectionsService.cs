using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using OptiDrive.Web.Models;

namespace OptiDrive.Web.Services;

public sealed class GoogleDirectionsService(HttpClient httpClient, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GoogleRouteResult?> GetDrivingRouteAsync(
        string origin,
        string destination,
        IReadOnlyList<string> waypoints,
        bool avoidTolls,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["GoogleMaps:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return null;
        }

        var query = new Dictionary<string, string?>
        {
            ["origin"] = origin,
            ["destination"] = destination,
            ["mode"] = "driving",
            ["language"] = "pt-PT",
            ["region"] = "pt",
            ["units"] = "metric",
            ["key"] = apiKey
        };

        if (avoidTolls)
        {
            query["avoid"] = "tolls";
        }

        var cleanedWaypoints = waypoints
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();

        if (cleanedWaypoints.Count > 0)
        {
            query["waypoints"] = string.Join('|', cleanedWaypoints);
        }

        var url = QueryHelpers.AddQueryString("https://maps.googleapis.com/maps/api/directions/json", query);
        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GoogleDirectionsResponse>(content, JsonOptions, cancellationToken);
        var route = payload?.Routes?.FirstOrDefault();
        if (route is null || !string.Equals(payload?.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var distanceMeters = route.Legs?.Sum(leg => leg.Distance?.Value ?? 0) ?? 0;
        var durationSeconds = route.Legs?.Sum(leg => leg.Duration?.Value ?? 0) ?? 0;
        var points = DecodePolyline(route.OverviewPolyline?.Points ?? string.Empty);
        if (points.Count == 0)
        {
            return null;
        }

        return new GoogleRouteResult
        {
            OriginAddress = route.Legs?.FirstOrDefault()?.StartAddress ?? origin,
            DestinationAddress = route.Legs?.LastOrDefault()?.EndAddress ?? destination,
            DistanceKm = Math.Round(distanceMeters / 1000d, 1),
            DurationMinutes = (int)Math.Round(durationSeconds / 60d),
            Path = points.Select((point, index) => new GeocodedPlace
            {
                Query = index == 0 ? origin : destination,
                DisplayName = index == 0 ? origin : index == points.Count - 1 ? destination : $"Ponto {index + 1}",
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                Provider = "Google Directions"
            }).ToList()
        };
    }

    private static List<(double Latitude, double Longitude)> DecodePolyline(string encoded)
    {
        var poly = new List<(double Latitude, double Longitude)>();
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return poly;
        }

        var index = 0;
        var lat = 0;
        var lng = 0;

        while (index < encoded.Length)
        {
            lat += DecodeNext(encoded, ref index);
            lng += DecodeNext(encoded, ref index);
            poly.Add((lat / 1E5, lng / 1E5));
        }

        return poly;
    }

    private static int DecodeNext(string encoded, ref int index)
    {
        var result = 0;
        var shift = 0;
        int b;

        do
        {
            if (index >= encoded.Length)
            {
                return 0;
            }

            b = encoded[index++] - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        }
        while (b >= 0x20 && index < encoded.Length);

        return (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
    }

    private sealed class GoogleDirectionsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("routes")]
        public List<GoogleDirectionsRoute> Routes { get; set; } = [];
    }

    private sealed class GoogleDirectionsRoute
    {
        [JsonPropertyName("overview_polyline")]
        public GoogleOverviewPolyline? OverviewPolyline { get; set; }

        [JsonPropertyName("legs")]
        public List<GoogleDirectionsLeg>? Legs { get; set; }
    }

    private sealed class GoogleOverviewPolyline
    {
        [JsonPropertyName("points")]
        public string Points { get; set; } = string.Empty;
    }

    private sealed class GoogleDirectionsLeg
    {
        [JsonPropertyName("distance")]
        public GoogleDistanceValue? Distance { get; set; }

        [JsonPropertyName("duration")]
        public GoogleDistanceValue? Duration { get; set; }

        [JsonPropertyName("start_address")]
        public string StartAddress { get; set; } = string.Empty;

        [JsonPropertyName("end_address")]
        public string EndAddress { get; set; } = string.Empty;
    }

    private sealed class GoogleDistanceValue
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}

public sealed class GoogleRouteResult
{
    public string OriginAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public IReadOnlyList<GeocodedPlace> Path { get; set; } = [];
}
