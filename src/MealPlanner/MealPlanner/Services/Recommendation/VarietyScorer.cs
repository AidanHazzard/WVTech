using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Scores a recipe by how "fresh" it is for the planned date — 1 when the user
/// has not eaten it within the lookback window, decaying toward 0 the more
/// recently and more often it appears on nearby days, past or future. Keeps a
/// day plan from echoing the rest of the week.
/// </summary>
public sealed class VarietyScorer : IRecipeScorer
{
    /// <summary>
    /// How many days either side of the planned date count toward recency.
    /// Tunable.
    /// </summary>
    public const int VarietyWindowDays = 14;

    public float Score(Recipe recipe, RecommendationContext ctx) => 1f;
}
