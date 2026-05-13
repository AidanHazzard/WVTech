using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Gives a large bonus to recipes the user has personally upvoted, ensuring they
/// rank ahead of any community vote percentage (which is capped at 1.0).
/// </summary>
public sealed class UpvotePriorityScorer : IRecipeScorer
{
    private const float UpvoteBonus = 100f;

    public float Score(Recipe recipe, RecommendationContext ctx) =>
        ctx.Upvoted.Any(r => r.Id == recipe.Id) ? UpvoteBonus : 0f;
}
