using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Returns a non-negative score for a recipe given the current recommendation context.
/// Higher scores rank the recipe earlier in the candidate list.
/// Implementations should be stateless and composable.
/// </summary>
public interface IRecipeScorer
{
    float Score(Recipe recipe, RecommendationContext ctx);
}
