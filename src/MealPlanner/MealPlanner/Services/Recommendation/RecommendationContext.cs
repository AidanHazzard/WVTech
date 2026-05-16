namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Bundles the per-user and per-meal-slot inputs passed to streams, scorers,
/// and filters. The user context is reused across every slot in a day plan;
/// the meal context is built fresh for each slot.
/// </summary>
public sealed record RecommendationContext(
    UserRecommendationContext User,
    MealRecommendationContext Meal);
