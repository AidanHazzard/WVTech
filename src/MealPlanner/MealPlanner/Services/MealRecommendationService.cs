using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Services;

public class MealRecommendationService : IMealRecommendationService
{
    private const int _MAX_RECIPES = 5;
    private IUserRecipeRepository _userRecipeRepository;
    private IRecipeRepository _recipeRepository;
    private IUserNutritionPreferenceRepository _nutrionRepository;
    private IMealRepository _mealRepository;
    private IExternalRecipeService? _externalRecipeService;
    
    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository, 
        IRecipeRepository recipeRepository, 
        IUserNutritionPreferenceRepository nutritionRepository,
        IMealRepository mealRepository,
        IExternalRecipeService? externalRecipeService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _recipeRepository = recipeRepository;
        _nutrionRepository = nutritionRepository;
        _mealRepository = mealRepository;
        _externalRecipeService = externalRecipeService;
    }

    public async Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate)
    {  
        var existingRecipes = (await _mealRepository.GetUserMealsByDateAsync(user, mealDate)).SelectMany(m => m.Recipes);
        var recipes = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        recipes = recipes.Where(r => !existingRecipes.Contains(r)).ToList();

        var rest = _recipeRepository
            .ReadAll()
            .Where(r => _userRecipeRepository.GetUserRecipeVoteAsync(user.Id, r.Id).Result != UserVoteType.DownVote && !existingRecipes.Contains(r) && !recipes.Contains(r))
            .OrderByDescending(r => _userRecipeRepository.GetRecipeVotePercentage(r.Id).Result)
            .ToList();

        recipes.AddRange(rest);

        // Target of zero implies no calorie limit
        var calorieTarget = (await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id))?.CalorieTarget ?? int.MaxValue;
        calorieTarget -= existingRecipes.Sum(r => r.Calories);
        List<Recipe> toReturn = [];
        int runningMealCalorieCount = 0;
        while(!recipes.IsNullOrEmpty() && toReturn.Count < _MAX_RECIPES)
        {
            Recipe? toAdd;
            (toAdd, runningMealCalorieCount) = await GetOneRecipeRecommendation(calorieTarget, recipes, runningMealCalorieCount);
            if (toAdd == null) continue;
            toReturn.Add(toAdd);
        }
        return toReturn;
    }

    private async Task<(Recipe? Recommendation, int mealCalories)> GetOneRecipeRecommendation(int calorieTarget, List<Recipe> recipes, int runningMealCalorieCount)
    {
        var toReturn = recipes.First();
        recipes.Remove(toReturn);
        if (!toReturn.ExternalUri.IsNullOrEmpty())
        {
            toReturn = await _externalRecipeService?.GetExternalRecipeByURI(toReturn.ExternalUri) ?? null;
        }

        if (toReturn != null && toReturn.Calories + runningMealCalorieCount <= calorieTarget)
        {
            return (toReturn, runningMealCalorieCount + toReturn.Calories);
        }
        return (null, runningMealCalorieCount);
    }

    public Task<List<Meal>> GetRecommendedDayPlanForUser(User user, DateTime mealDate, DayPlanConfigViewModel config)
    {
        throw new NotImplementedException();
    }
}