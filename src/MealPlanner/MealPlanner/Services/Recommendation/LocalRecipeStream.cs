using MealPlanner.DAL.Abstract;
using MealPlanner.Helpers;
using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public sealed class LocalRecipeStream : IRecommendationStream
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IExternalRecipeService? _externalRecipeService;

    public LocalRecipeStream(IRecipeRepository recipeRepository, IExternalRecipeService? externalRecipeService = null)
    {
        _recipeRepository = recipeRepository;
        _externalRecipeService = externalRecipeService;
    }

    public async Task<List<Recipe>> GetCandidatesAsync()
    {
        var recipes = await _recipeRepository.GetAllWithTagsAsync();
        await recipes.LoadExternalRecipesAsync(_externalRecipeService);
        return recipes;
    }
}
