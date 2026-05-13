using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Per-user recommendation inputs assembled once per request. Carries the user's
/// dietary restrictions, vote history, upvoted recipes, and standing food-tag
/// preferences. Streams and scorers read from this for personalisation.
/// </summary>
public sealed record UserRecommendationContext(
    HashSet<string> RestrictionNames,
    Dictionary<int, UserVoteType> UserVotes,
    Dictionary<int, float> VotePercentages,
    List<Recipe> Upvoted,
    HashSet<int> PreferredTagIds);
