using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Per-meal-slot inputs. Carries the slot's calorie and macro targets and any
/// tag preferences explicitly requested for this slot. Built fresh for each
/// meal in a day plan and passed alongside <see cref="UserRecommendationContext"/>.
/// </summary>
public sealed record MealRecommendationContext(
    int? CalorieTarget,
    int? ProteinTarget,
    int? CarbTarget,
    int? FatTarget,
    HashSet<int> PreferredTagIds);
