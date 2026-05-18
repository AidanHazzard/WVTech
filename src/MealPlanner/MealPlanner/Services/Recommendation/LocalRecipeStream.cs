using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public sealed class LocalRecipeStream : IRecommendationStream
{
    // A non-upvoted local recipe whose score normalizes below this fraction of
    // the candidate score range is dropped, so on-context external recipes can
    // take its place. Tunable.
    private const float LocalScoreFloor = 0.25f;

    private readonly IRecipeRepository _recipeRepository;
    private readonly IReadOnlyList<IRecipeScorer> _scorers;
    private readonly IReadOnlyList<IRecipeFilter> _filters;

    public LocalRecipeStream(
        IRecipeRepository recipeRepository,
        IEnumerable<IRecipeScorer> scorers,
        IEnumerable<IRecipeFilter> filters)
    {
        _recipeRepository = recipeRepository;
        _scorers = scorers.ToList();
        _filters = filters.ToList();
    }

    public async Task<IReadOnlyList<ScoredRecipe>> GetRankedCandidatesAsync(RecommendationContext ctx)
    {
        var recipes = await _recipeRepository.GetAllWithTagsAndIngredientsAsync();
        var upvotedIds = ctx.User.Upvoted.Select(r => r.Id).ToHashSet();

        var scored = recipes
            .Where(r => _filters.All(f => f.Allow(r, ctx)))
            .Select(r => (Recipe: r, Score: _scorers.Sum(s => s.Score(r, ctx))))
            .ToList();

        return ApplyScoreFloor(scored, upvotedIds)
            .OrderByDescending(x => x.Score)
            .Select(x => new ScoredRecipe(x.Recipe, x.Score))
            .ToList();
    }

    // Drops weak non-upvoted recipes: scores are min-max normalized across the
    // non-upvoted candidates and anything below the floor is removed. Upvoted
    // recipes are kept regardless. When every score is equal there is no range
    // to normalize against, so nothing is dropped.
    private static IEnumerable<(Recipe Recipe, float Score)> ApplyScoreFloor(
        List<(Recipe Recipe, float Score)> scored,
        HashSet<int> upvotedIds)
    {
        var nonUpvotedScores = scored
            .Where(x => !upvotedIds.Contains(x.Recipe.Id))
            .Select(x => x.Score)
            .ToList();
        if (nonUpvotedScores.Count == 0) return scored;

        float min = nonUpvotedScores.Min();
        float max = nonUpvotedScores.Max();
        if (max <= min) return scored;

        return scored.Where(x =>
            upvotedIds.Contains(x.Recipe.Id)
            || (x.Score - min) / (max - min) >= LocalScoreFloor);
    }
}
