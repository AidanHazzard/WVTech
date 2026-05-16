using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Helpers;

public static class MealListExtensions
{
    public static async Task LoadExternalRecipesAsync(this List<Meal> meals, IExternalRecipeService? externalRecipeService)
    {
        if (externalRecipeService == null) return;
        
        var mealRecipes = meals.SelectMany(m => m.Recipes).ToList();
        await mealRecipes.LoadExternalRecipesAsync(externalRecipeService);

        foreach(var meal in meals)
        {
            for (int i = 0; i < meal.Recipes.Count; i++)
            {
                if (meal.Recipes[i].ExternalUri.IsNullOrEmpty()) continue;
                var external = mealRecipes.Find(r => r.ExternalUri == meal.Recipes[i].ExternalUri);
                if (external == null) continue;
                meal.Recipes[i] = external;
            }
        }
    }
}