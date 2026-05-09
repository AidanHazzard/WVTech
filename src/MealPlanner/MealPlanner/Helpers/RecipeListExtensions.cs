using MealPlanner.Models;
using MealPlanner.Services;

namespace MealPlanner.Helpers;

public static class RecipeListExtensions
{
    public static async Task LoadExternalRecipesAsync(this List<Recipe> recipes, IExternalRecipeService? externalRecipeService)
    {
        if (externalRecipeService == null) return;

        var external = recipes.Where(r => !string.IsNullOrEmpty(r.ExternalUri)).ToList();
        if (external.Count == 0) return;

        Dictionary<string, Recipe> loaded;
        try
        {
            var results = await externalRecipeService.GetExternalRecipesByURIs(
                external.Select(r => r.ExternalUri!));
            loaded = results.ToDictionary(r => r.ExternalUri!);
        }
        catch
        {
            foreach (var r in external)
                recipes.Remove(r);
            return;
        }

        for (int i = recipes.Count - 1; i >= 0; i--)
        {
            var r = recipes[i];
            if (string.IsNullOrEmpty(r.ExternalUri)) continue;

            if (loaded.TryGetValue(r.ExternalUri, out var loadedRecipe))
            {
                loadedRecipe.Id = r.Id;
                recipes[i] = loadedRecipe;
            }
            else
            {
                recipes.RemoveAt(i);
            }
        }
    }
}
