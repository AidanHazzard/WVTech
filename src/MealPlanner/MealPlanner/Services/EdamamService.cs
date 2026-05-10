using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using MealPlanner.Helpers;
using MealPlanner.Models;
using MealPlanner.Models.DTO;

namespace MealPlanner.Services;

public class EdamamService:IExternalRecipeService
{
    HttpClient _httpClient;
    JsonSerializerOptions _responseDeserializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    string _appId;
    string _apiKey;

    public EdamamService(HttpClient httpClient, string appId, string apiKey)
    {
        _httpClient = httpClient;
        _appId = appId;
        _apiKey = apiKey;
    }
    
    public async Task<IEnumerable<RecipeDTO>> SearchExternalRecipesByName(string recipeName)
    {
        string endpoint = $"recipes/v2?type=any&q={recipeName}&app_id={_appId}&app_key={_apiKey}&field=uri&field=label&field=image";

        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error accessing Edamam Recipe Search API: {response.StatusCode}");
        }
        string responseBody = await response.Content.ReadAsStringAsync();

        EdamamRecipeSearchResponse? edamamResponse = JsonSerializer.Deserialize<EdamamRecipeSearchResponse>
        (
            responseBody,
            _responseDeserializerOptions
        );
        return edamamResponse?.Hits.Select(e => new RecipeDTO { Name = e.Recipe.Label, ExternalUri = e.Recipe.Uri, ImageUrl = e.Recipe.Image }) ?? [];
    }

    public async Task<Recipe?> GetExternalRecipeByURI(string uri)
    {
        uri = WebUtility.UrlEncode(uri);
        string endpoint = $"recipes/v2/by-uri?uri={uri}&app_id={_appId}&app_key={_apiKey}";
        
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.RequestMessage);
            throw new Exception($"Error accessing Edamam Recipe Search API: {response.StatusCode}");
        }
        string responseBody = await response.Content.ReadAsStringAsync();
        
        EdamamRecipeSearchResponse? edamamResponse = JsonSerializer.Deserialize<EdamamRecipeSearchResponse>
        (
            responseBody,
            _responseDeserializerOptions
        );
        return edamamResponse?.Hits.Select(
            e => new Recipe
            {
                Name = e.Recipe.Label,
                ExternalUri = e.Recipe.Uri,
                Directions = "",
                Calories = (int?) e.Recipe.TotalNutrients?["ENERC_KCAL"]?.Quantity ?? 0,
                Protein = (int?) e.Recipe.TotalNutrients?["PROCNT"]?.Quantity ?? 0,
                Carbs = (int?) e.Recipe.TotalNutrients?["CHOCDF"]?.Quantity ?? 0,
                Fat = (int?) e.Recipe.TotalNutrients?["FAT"]?.Quantity ?? 0,
                ImageUrl = e.Recipe.Image,
                Ingredients = ParseIngredientsFromResponse(e.Recipe.Ingredients ?? [])
            }).FirstOrDefault();
    }

    public async Task<IEnumerable<Recipe>> GetExternalRecipesByURIs(IEnumerable<string> uris)
    {
        var uriList = uris.ToList();
        if (uriList.Count == 0)
            return [];

        var results = new List<Recipe>();
        foreach (var batch in uriList.Chunk(20))
        {
            string uriParams = string.Join("&", batch.Select(u => $"uri={WebUtility.UrlEncode(u)}"));
            string endpoint = $"recipes/v2/by-uri?{uriParams}&app_id={_appId}&app_key={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error accessing Edamam Recipe Search API: {response.StatusCode}");

            string responseBody = await response.Content.ReadAsStringAsync();
            EdamamRecipeSearchResponse? edamamResponse = JsonSerializer.Deserialize<EdamamRecipeSearchResponse>(
                responseBody, _responseDeserializerOptions);

            if (edamamResponse is not null)
                results.AddRange(edamamResponse.Hits.Select(e => new Recipe
                {
                    Name = e.Recipe.Label,
                    ExternalUri = e.Recipe.Uri,
                    Directions = "",
                    Calories = (int?) e.Recipe.TotalNutrients?["ENERC_KCAL"]?.Quantity ?? 0,
                    Protein  = (int?) e.Recipe.TotalNutrients?["PROCNT"]?.Quantity ?? 0,
                    Carbs    = (int?) e.Recipe.TotalNutrients?["CHOCDF"]?.Quantity ?? 0,
                    Fat      = (int?) e.Recipe.TotalNutrients?["FAT"]?.Quantity ?? 0,
                    ImageUrl = e.Recipe.Image,
                    Ingredients = ParseIngredientsFromResponse(e.Recipe.Ingredients ?? [])
                }));
        }
        return results;
    }

    private List<Ingredient> ParseIngredientsFromResponse(IList<EdamamIngredient> edamamIngredients)
    {
        return edamamIngredients.Select(i => new Ingredient
        {
            DisplayName = i.Food,
            Amount = i.Quantity,
            IngredientBase = new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(i.Food) },
            Measurement = new Measurement { Name = i.Measure }
        }).ToList();
    }
}