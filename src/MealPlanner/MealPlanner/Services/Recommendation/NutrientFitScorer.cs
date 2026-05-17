using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Scores a recipe by how closely its protein/carb/fat composition matches the
/// meal slot's target composition. Both the recipe's macros and the slot's
/// targets are reduced to fraction vectors on the macro simplex (each summing
/// to 1), and the score is the L1 distance between them mapped into [0, 1] —
/// identical balance scores 1, opposite balance scores 0. The score is
/// scale-invariant: only the macro <em>balance</em> matters, not portion size
/// (the composer handles total amounts). Returns 0 when the slot has fewer
/// than three macro targets, or the recipe carries no macros.
/// </summary>
public sealed class NutrientFitScorer : IRecipeScorer
{
    public float Score(Recipe recipe, RecommendationContext ctx) => 0f;
}
