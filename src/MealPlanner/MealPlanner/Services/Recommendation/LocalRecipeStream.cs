using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public sealed class LocalRecipeStream : IRecommendationStream
{
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

    public async Task<IEnumerable<Recipe>> GetRankedCandidatesAsync(RecommendationContext ctx)
    {
        var recipes = await _recipeRepository.GetAllWithTagsAsync();
        return recipes
            .Where(r => _filters.All(f => f.Allow(r, ctx)))
            .OrderByDescending(r => _scorers.Sum(s => s.Score(r, ctx)));
    }
}
