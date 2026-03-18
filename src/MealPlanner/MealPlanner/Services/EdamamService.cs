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
        string endpoint = $"recipes/v2?type=any&q={recipeName}&app_id={_appId}&app_key={_apiKey}&field=uri&field=label";

        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error accessing Edamam Recipe Search API: {response.StatusCode}");
        }
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Edamam Response");
        Console.WriteLine(responseBody);
        EdamamRecipeSearchResponse? edamamResponse = JsonSerializer.Deserialize<EdamamRecipeSearchResponse>
        (
            responseBody,
            _responseDeserializerOptions
        );
        return edamamResponse?.Hits.Select(e => new RecipeDTO { Name = e.Recipe.Label, ExternalUri = e.Recipe.Uri }) ?? [];
    }

    public async Task<Recipe?> GetExternalRecipeByURI(string uri)
    {
        uri = WebUtility.UrlEncode(uri);
        string endpoint = $"recipes/v2/by-uri?uri={uri}&app_id={_appId}&app_key={_apiKey}";
        
        Console.WriteLine(endpoint);
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.RequestMessage);
            throw new Exception($"Error accessing Edamam Recipe Search API: {response.StatusCode}");
        }
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Edamam Response");
        Console.WriteLine(responseBody);
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
                Ingredients = ParseIngredientsFromResponse(e.Recipe.Ingredients ?? [])
            }).FirstOrDefault();
    }

    private List<Ingredient> ParseIngredientsFromResponse(IList<EdamamIngredient> edamamIngredients)
    {
        return edamamIngredients.Select(i => new Ingredient
        {
            Amount = i.Quantity,
            IngredientBase = new IngredientBase { Name = i.Food },
            Measurement = new Measurement { Name = i.Measure }
        }).ToList();
    }
}