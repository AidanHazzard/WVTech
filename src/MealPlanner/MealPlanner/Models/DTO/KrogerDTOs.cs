using System.Text.Json.Serialization;

namespace MealPlanner.Models.DTO;

public class KrogerTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class KrogerCartItem
{
    public string Upc { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class KrogerStoreInfo
{
    public string LocationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

internal class KrogerLocationResponse
{
    [JsonPropertyName("data")]
    public List<KrogerLocationData> Data { get; set; } = [];
}

internal class KrogerLocationData
{
    [JsonPropertyName("locationId")]
    public string LocationId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public KrogerLocationAddress? Address { get; set; }
}

internal class KrogerLocationAddress
{
    [JsonPropertyName("addressLine1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("zipCode")]
    public string ZipCode { get; set; } = string.Empty;
}

internal class KrogerProductResponse
{
    [JsonPropertyName("data")]
    public List<KrogerProductData> Data { get; set; } = [];
}

internal class KrogerProductData
{
    [JsonPropertyName("upc")]
    public string Upc { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<KrogerProductItem> Items { get; set; } = [];
}

internal class KrogerProductItem
{
    [JsonPropertyName("size")]
    public string? Size { get; set; }
}

public class KrogerProductMatch
{
    public string Upc { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class SaveZipRequest
{
    public string? ZipCode { get; set; }
}
