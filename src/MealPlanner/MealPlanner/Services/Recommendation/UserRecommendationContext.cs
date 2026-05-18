using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Per-user recommendation inputs assembled once per request. Carries the user's
/// dietary restrictions, vote history, upvoted recipes, standing food-tag
/// preferences, and the normalized names of ingredients in their pantry.
/// Streams and scorers read from this for personalisation.
/// </summary>
public sealed record UserRecommendationContext(
    HashSet<string> RestrictionNames,
    Dictionary<int, UserVoteType> UserVotes,
    Dictionary<int, float> VotePercentages,
    List<Recipe> Upvoted,
    HashSet<int> PreferredTagIds,
    HashSet<string> PantryIngredientNames);
