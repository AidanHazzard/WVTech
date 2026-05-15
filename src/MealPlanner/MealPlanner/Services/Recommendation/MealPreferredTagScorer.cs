using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Soft preference scorer based on the explicit tag preferences for this meal
/// slot. Returns the fraction of <see cref="MealRecommendationContext.PreferredTagIds"/>
/// that this recipe matches, in [0, 1]. Returns 0 when the slot has no
/// preferred tags. Works alongside <see cref="PreferredTagFilter"/>, which
/// removes recipes that match no preferred tags at all.
/// </summary>
public sealed class MealPreferredTagScorer : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx)
    {
        var preferred = ctx.Meal.PreferredTagIds;
        if (preferred.Count == 0) return 0f;
        int matches = recipe.Tags.Count(t => preferred.Contains(t.Id));
        return (float)matches / preferred.Count;
    }
}
