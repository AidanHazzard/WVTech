using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Hard floor for the meal-slot tag intent. When the slot specifies preferred
/// tags, a recipe must match at least one of them to qualify. When the slot
/// has no preferred tags, every recipe passes. Prevents off-theme local
/// recipes (e.g. desserts at breakfast) from crowding out on-theme external
/// candidates.
/// </summary>
public sealed class PreferredTagFilter : IRecipeFilter
{
    public bool Allow(Recipe recipe, RecommendationContext ctx)
    {
        var preferred = ctx.Meal.PreferredTagIds;
        return preferred.Count == 0
            || recipe.Tags.Any(t => preferred.Contains(t.Id));
    }
}
