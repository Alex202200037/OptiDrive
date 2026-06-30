using System.Text.Json.Serialization;

namespace OptiDrive.Web.Models;

public sealed class VpicResponse<T>
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Results")]
    public List<T> Results { get; set; } = [];
}

public sealed class VpicMakeItem
{
    [JsonPropertyName("MakeId")]
    public int MakeId { get; set; }

    [JsonPropertyName("MakeName")]
    public string MakeName { get; set; } = string.Empty;
}

public sealed class VpicModelItem
{
    [JsonPropertyName("Model_ID")]
    public int ModelId { get; set; }

    [JsonPropertyName("Model_Name")]
    public string ModelName { get; set; } = string.Empty;
}

public sealed class DgegResponse<T>
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("mensagem")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("resultado")]
    public List<T> Result { get; set; } = [];
}

public sealed class DgegFuelTypeItem
{
    [JsonPropertyName("Descritivo")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("fl_ViewWebSite")]
    public bool ViewWebsite { get; set; }

    [JsonPropertyName("fl_rodoviario")]
    public bool RoadVehicle { get; set; }

    [JsonPropertyName("fl_ativo")]
    public bool Active { get; set; }

    [JsonPropertyName("Id")]
    public int Id { get; set; }
}

public sealed class DgegStationItem
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Nome")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Marca")]
    public string Brand { get; set; } = string.Empty;

    [JsonPropertyName("Municipio")]
    public string Municipality { get; set; } = string.Empty;

    [JsonPropertyName("Distrito")]
    public string District { get; set; } = string.Empty;

    [JsonPropertyName("Combustivel")]
    public string Fuel { get; set; } = string.Empty;

    [JsonPropertyName("Preco")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("Latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("DataAtualizacao")]
    public string UpdatedAt { get; set; } = string.Empty;

    [JsonPropertyName("Quantidade")]
    public int TotalQuantity { get; set; }

    [JsonPropertyName("TipoPosto")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("Morada")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("Localidade")]
    public string Locality { get; set; } = string.Empty;
}

public sealed class NominatimSearchItem
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public string Latitude { get; set; } = string.Empty;

    [JsonPropertyName("lon")]
    public string Longitude { get; set; } = string.Empty;
}

public sealed class OpenChargeMapItem
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("AddressInfo")]
    public OpenChargeMapAddressInfo? AddressInfo { get; set; }

    [JsonPropertyName("Connections")]
    public List<OpenChargeMapConnection>? Connections { get; set; }

    [JsonPropertyName("OperatorInfo")]
    public OpenChargeMapOperatorInfo? OperatorInfo { get; set; }

    [JsonPropertyName("UsageCost")]
    public string? UsageCost { get; set; }
}

public sealed class OpenChargeMapAddressInfo
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("AddressLine1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("Town")]
    public string? Town { get; set; }

    [JsonPropertyName("StateOrProvince")]
    public string? StateOrProvince { get; set; }

    [JsonPropertyName("Latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double Longitude { get; set; }
}

public sealed class OpenChargeMapConnection
{
    [JsonPropertyName("ConnectionType")]
    public OpenChargeMapConnectionType? ConnectionType { get; set; }

    [JsonPropertyName("PowerKW")]
    public double? PowerKw { get; set; }
}

public sealed class OpenChargeMapConnectionType
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}

public sealed class OpenChargeMapOperatorInfo
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}
