using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Soft preference scorer based on the user's standing food-tag preferences.
/// Returns the fraction of <see cref="UserRecommendationContext.PreferredTagIds"/>
/// that this recipe matches, in [0, 1]. Returns 0 when the user has no
/// preferred tags configured.
/// </summary>
public sealed class UserPreferredTagScorer : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx)
    {
        var preferred = ctx.User.PreferredTagIds;
        if (preferred.Count == 0) return 0f;
        int matches = recipe.Tags.Count(t => preferred.Contains(t.Id));
        return (float)matches / preferred.Count;
    }
}
