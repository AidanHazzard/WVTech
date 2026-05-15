using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

/// <summary>
/// Recommendation stream backed by an external recipe provider (Edamam).
/// Composes a search query from the current <see cref="RecommendationContext"/>
/// — meal calorie/macro targets, the user's dietary restrictions, and any
/// preferred tag names — then applies the same scorers/filters as the local
/// stream so external candidates can be merged into the recommendation pool.
/// Returns an empty result when no external service is registered, when the
/// composed query has no criteria, or when the API call fails.
/// </summary>
public sealed class ExternalRecipeStream : IRecommendationStream
{
    private readonly IExternalRecipeService? _externalRecipeService;
    private readonly ITagRepository _tagRepository;
    private readonly IReadOnlyList<IRecipeScorer> _scorers;
    private readonly IReadOnlyList<IRecipeFilter> _filters;

    public ExternalRecipeStream(
        ITagRepository tagRepository,
        IEnumerable<IRecipeScorer> scorers,
        IEnumerable<IRecipeFilter> filters,
        IExternalRecipeService? externalRecipeService = null)
    {
        _tagRepository = tagRepository;
        _scorers = scorers.ToList();
        _filters = filters.ToList();
        _externalRecipeService = externalRecipeService;
    }

    public async Task<IEnumerable<Recipe>> GetRankedCandidatesAsync(RecommendationContext ctx)
    {
        if (_externalRecipeService == null) return [];

        var query = await BuildQueryAsync(ctx);
        if (!query.HasAnyCriteria) return [];

        IEnumerable<Recipe> recipes;
        try
        {
            recipes = await _externalRecipeService.SearchByContextAsync(query);
        }
        catch
        {
            return [];
        }

        return recipes
            .Where(r => _filters.All(f => f.Allow(r, ctx)))
            .OrderByDescending(r => _scorers.Sum(s => s.Score(r, ctx)));
    }

    private async Task<ExternalSearchQuery> BuildQueryAsync(RecommendationContext ctx)
    {
        var tagIds = ctx.Meal.PreferredTagIds.Union(ctx.User.PreferredTagIds).ToList();
        string? freeText = null;
        if (tagIds.Count > 0)
        {
            var tags = await _tagRepository.GetTagsByIdsAsync(tagIds);
            if (tags.Count > 0)
                freeText = string.Join(" ", tags.Select(t => t.Name));
        }

        var healthFilters = ctx.User.RestrictionNames
            .Select(n => n.ToLowerInvariant())
            .ToList();

        return new ExternalSearchQuery(
            freeText,
            CaloriesMin: ctx.Meal.CalorieTarget.HasValue ? 1 : (int?)null,
            CaloriesMax: ctx.Meal.CalorieTarget,
            ProteinMin: ctx.Meal.ProteinTarget.HasValue ? 0 : (int?)null,
            ProteinMax: ctx.Meal.ProteinTarget,
            CarbsMin: ctx.Meal.CarbTarget.HasValue ? 0 : (int?)null,
            CarbsMax: ctx.Meal.CarbTarget,
            FatMin: ctx.Meal.FatTarget.HasValue ? 0 : (int?)null,
            FatMax: ctx.Meal.FatTarget,
            healthFilters);
    }
}
