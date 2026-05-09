using MealPlanner.Models;
using MealPlanner.Services;

namespace MealPlanner.Helpers;

public static class RecipeListExtensions
{
    public static async Task LoadExternalRecipesAsync(this List<Recipe> recipes, IExternalRecipeService? externalRecipeService)
    {
        if (externalRecipeService == null) return;

        for (int i = recipes.Count - 1; i >= 0; i--)
        {
            var r = recipes[i];
            if (string.IsNullOrEmpty(r.ExternalUri)) continue;
            try
            {
                var loaded = await externalRecipeService.GetExternalRecipeByURI(r.ExternalUri);
                if (loaded != null)
                {
                    loaded.Id = r.Id;
                    recipes[i] = loaded;
                }
                else
                {
                    recipes.RemoveAt(i);
                }
            }
            catch
            {
                recipes.RemoveAt(i);
            }
        }
    }
}
