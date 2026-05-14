using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Hard constraint that excludes a recipe from the candidate set entirely.
/// Returns true if the recipe is allowed through; false to reject it.
/// </summary>
public interface IRecipeFilter
{
    bool Allow(Recipe recipe, RecommendationContext ctx);
}
