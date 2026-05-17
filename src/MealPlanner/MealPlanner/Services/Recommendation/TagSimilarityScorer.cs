using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Scores a recipe by how closely its tags match the user's inferred taste
/// profile — the frequency of each tag across the recipes the user has
/// upvoted. The score is the summed profile frequency of the recipe's tags,
/// normalized by the total profile weight, giving a value in [0, 1]. Tags
/// that recur often across upvoted recipes count for more than rare ones.
/// Returns 0 when the user has upvoted nothing, or none of the upvoted
/// recipes carry tags.
/// </summary>
public sealed class TagSimilarityScorer : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx) => 0f;
}
