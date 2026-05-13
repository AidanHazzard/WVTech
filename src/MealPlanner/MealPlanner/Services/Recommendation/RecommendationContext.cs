using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// User-specific data assembled once per request. Passed to streams and scorers so they
/// can personalise ranking without additional DB calls. Does not carry the recipe pool —
/// the stream is responsible for fetching and ranking candidates.
/// </summary>
public sealed record RecommendationContext(
    HashSet<string> RestrictionNames,
    Dictionary<int, UserVoteType> UserVotes,
    Dictionary<int, float> VotePercentages,
    List<Recipe> Upvoted);
