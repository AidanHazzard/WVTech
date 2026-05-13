using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Per-request data bundle assembled once by BuildContextAsync and shared across all
/// ordering/selection steps in the recommendation pipeline.
/// </summary>
public sealed record RecommendationContext(
    HashSet<string> RestrictionNames,
    Dictionary<int, UserVoteType> UserVotes,
    Dictionary<int, float> VotePercentages,
    List<Recipe> Upvoted,
    List<Recipe> AllWithTags);
