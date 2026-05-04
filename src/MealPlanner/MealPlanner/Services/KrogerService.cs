using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MealPlanner.Models.DTO;
using Microsoft.Extensions.Configuration;

namespace MealPlanner.Services;

public class KrogerService : IKrogerService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public KrogerService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _clientId = configuration["Kroger:ClientId"] ?? string.Empty;
        _clientSecret = configuration["Kroger:ClientSecret"] ?? string.Empty;
        _redirectUri = configuration["Kroger:RedirectUri"] ?? string.Empty;
    }

    public string GetAuthorizationUrl(string state) =>
        "https://api.kroger.com/v1/connect/oauth2/authorize" +
        $"?response_type=code" +
        $"&client_id={Uri.EscapeDataString(_clientId)}" +
        $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
        $"&scope={Uri.EscapeDataString("cart.basic:write")}" +
        $"&state={Uri.EscapeDataString(state)}";

    public async Task<KrogerTokenResponse?> ExchangeCodeAsync(string code)
    {
        var request = BuildTokenRequest(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri)
        });

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KrogerTokenResponse>(json, JsonOptions);
    }

    public async Task<List<KrogerStoreInfo>> FindNearestStoresAsync(string zipCode, int limit = 5)
    {
        var token = await GetClientCredentialsTokenAsync();
        if (token == null) return [];

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"locations?filter.zipCode.near={Uri.EscapeDataString(zipCode)}&filter.limit={limit}&filter.radiusInMiles=50");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<KrogerLocationResponse>(json, JsonOptions);

        return result?.Data.Select(d => new KrogerStoreInfo
        {
            LocationId = d.LocationId,
            Name = d.Name,
            AddressLine1 = d.Address?.AddressLine1 ?? string.Empty,
            City = d.Address?.City ?? string.Empty,
            State = d.Address?.State ?? string.Empty,
            ZipCode = d.Address?.ZipCode ?? string.Empty
        }).ToList() ?? [];
    }

    public async Task<KrogerProductMatch?> SearchProductUpcAsync(
        string ingredientName, string storeId, float amount, string measurement)
    {
        var token = await GetClientCredentialsTokenAsync();
        if (token == null) return null;

        var term = NormalizeSearchTerm(ingredientName);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"products?filter.term={Uri.EscapeDataString(term)}&filter.locationId={storeId}&filter.limit=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<KrogerProductResponse>(json, JsonOptions);
        if (result == null || result.Data.Count == 0) return null;

        var termWords = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var best = result.Data
            .Where(p => termWords.All(w =>
                p.Description.Contains(w, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(p => p.Description.Length)
            .FirstOrDefault()
            ?? result.Data[0];

        var productSize = best.Items.FirstOrDefault()?.Size;
        var quantity = CalculateQuantity(amount, measurement, productSize);

        return new KrogerProductMatch { Upc = best.Upc, Quantity = quantity };
    }

    private static int CalculateQuantity(float recipeAmount, string measurement, string? productSize)
    {
        var isCount = string.IsNullOrWhiteSpace(measurement)
            || measurement.Equals("Count", StringComparison.OrdinalIgnoreCase);

        double productOz = ParseSizeToOz(productSize, out bool productIsCount);

        // Count ingredient vs count product — divide units
        if (isCount && productIsCount && productOz > 0)
            return Math.Max(1, (int)Math.Ceiling(recipeAmount / productOz));

        // Count ingredient with no usable product size — use raw amount
        if (isCount)
            return Math.Max(1, (int)Math.Ceiling(recipeAmount));

        // Volume/weight ingredient — convert both to oz and divide
        double recipeOz = measurement.ToLowerInvariant() switch
        {
            "cup(s)" => recipeAmount * 8,
            "ounce(s)" => recipeAmount,
            "pound(s)" => recipeAmount * 16,
            "l" => recipeAmount * 33.814,
            "kg" => recipeAmount * 35.274,
            _ => 0
        };

        if (recipeOz <= 0 || productOz <= 0 || productIsCount)
            return 1;

        return Math.Max(1, Math.Min(10, (int)Math.Ceiling(recipeOz / productOz)));
    }

    private static double ParseSizeToOz(string? size, out bool isCount)
    {
        isCount = false;
        if (string.IsNullOrWhiteSpace(size)) return 0;

        var m = Regex.Match(size.Trim(),
            @"^([\d.]+)\s*(fl\.?\s*oz|ounces?|oz|gallons?|gal|pounds?|lbs?|lb|quarts?|qt|pints?|pt|milliliters?|ml|liters?|litres?|l|ct|count|pk|pack)\b",
            RegexOptions.IgnoreCase);
        if (!m.Success || !double.TryParse(m.Groups[1].Value, out var n)) return 0;

        var unit = Regex.Replace(m.Groups[2].Value, @"\s+", "").ToLowerInvariant();
        switch (unit)
        {
            case "floz": case "fl.oz": case "oz": case "ounce": case "ounces": return n;
            case "lb": case "lbs": case "pound": case "pounds": return n * 16;
            case "qt": case "quart": case "quarts": return n * 32;
            case "gal": case "gallon": case "gallons": return n * 128;
            case "pt": case "pint": case "pints": return n * 16;
            case "l": case "liter": case "liters": case "litre": case "litres": return n * 33.814;
            case "ml": case "milliliter": case "milliliters": return n * 0.033814;
            case "ct": case "count": case "pk": case "pack":
                isCount = true;
                return n;
            default: return 0;
        }
    }

    private static string NormalizeSearchTerm(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name ?? string.Empty;

        string[] descriptors =
        [
            "single", "small", "medium", "large", "extra", "fresh", "organic", "whole",
            "sliced", "chopped", "diced", "minced", "frozen", "canned", "raw", "cooked",
            "ripe", "boneless", "skinless", "dried", "ground", "shredded", "grated",
            "peeled", "seedless", "baby", "mini", "jumbo"
        ];

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !descriptors.Contains(w, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        return words.Length > 0 ? string.Join(' ', words) : name;
    }

    public async Task<bool> ExportCartAsync(IEnumerable<KrogerCartItem> items, string userAccessToken)
    {
        var payload = new
        {
            items = items.Select(i => new { upc = i.Upc, quantity = i.Quantity, modality = "PICKUP" })
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "cart/add")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken) },
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task<string?> GetClientCredentialsTokenAsync()
    {
        var request = BuildTokenRequest(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "product.compact")
        });

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<KrogerTokenResponse>(json, JsonOptions);
        return token?.AccessToken;
    }

    private HttpRequestMessage BuildTokenRequest(IEnumerable<KeyValuePair<string, string>> fields)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "connect/oauth2/token")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", credentials) },
            Content = new FormUrlEncodedContent(fields)
        };
        return request;
    }
}
