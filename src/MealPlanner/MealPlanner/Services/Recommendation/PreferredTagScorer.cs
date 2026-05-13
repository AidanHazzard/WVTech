using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Scores a recipe by the fraction of the user's preferred tag IDs that it matches.
/// Returns 0 when no preferred tags are specified.
/// </summary>
public sealed class PreferredTagScorer(IReadOnlyList<int> preferredTagIds) : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx)
    {
        if (preferredTagIds.Count == 0) return 0f;
        int matches = recipe.Tags.Count(t => preferredTagIds.Contains(t.Id));
        return (float)matches / preferredTagIds.Count;
    }
}
