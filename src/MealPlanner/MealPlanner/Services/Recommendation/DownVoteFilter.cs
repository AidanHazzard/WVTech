using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Hard-rejects any recipe the user has explicitly downvoted.
/// </summary>
public sealed class DownVoteFilter : IRecipeFilter
{
    public bool Allow(Recipe recipe, RecommendationContext ctx) =>
        ctx.User.UserVotes.GetValueOrDefault(recipe.Id, UserVoteType.NoVote) != UserVoteType.DownVote;
}
