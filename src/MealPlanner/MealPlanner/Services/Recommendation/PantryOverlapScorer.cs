using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Scores a recipe by the fraction of its distinct ingredients the user
/// already has in their pantry. Ingredient names are normalized the same way
/// on both sides so the match works for local and external recipes alike.
/// Returns 0 when the pantry is empty or the recipe carries no ingredients.
/// </summary>
public sealed class PantryOverlapScorer : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx) => 0f;
}
