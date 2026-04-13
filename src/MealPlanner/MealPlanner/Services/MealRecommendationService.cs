using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Services;

public class MealRecommendationService : IMealRecommendationService
{
    private const int _MAX_RECIPES = 5;
    private IUserRecipeRepository _userRecipeRepository;
    private IRecipeRepository _recipeRepository;
    private IUserNutritionPreferenceRepository _nutrionRepository;
    private IMealRepository _mealRepository;
    
    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository, 
        IRecipeRepository recipeRepository, 
        IUserNutritionPreferenceRepository nutritionRepository,
        IMealRepository mealRepository)
    {
        _userRecipeRepository = userRecipeRepository;
        _recipeRepository = recipeRepository;
        _nutrionRepository = nutritionRepository;
        _mealRepository = mealRepository;
    }

    public async Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate)
    {  
        var existingRecipes = (await _mealRepository.GetUserMealsByDateAsync(user, mealDate)).SelectMany(m => m.Recipes);
        var recipes = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        recipes = recipes.Where(r => !existingRecipes.Contains(r)).ToList();

        var rest = _recipeRepository
            .ReadAll()
            .Where(r => _userRecipeRepository.GetUserRecipeVoteAsync(user.Id, r.Id).Result != UserVoteType.DownVote && !existingRecipes.Contains(r))
            .OrderByDescending(r => _userRecipeRepository.GetRecipeVotePercentage(r.Id).Result)
            .ToList();

        recipes.AddRange(rest);
        var calorieTarget = (await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id))?.CalorieTarget ?? int.MaxValue;
        calorieTarget -= existingRecipes.Sum(r => r.Calories);
        List<Recipe> toReturn = [];

        while(!recipes.IsNullOrEmpty() && toReturn.Count < _MAX_RECIPES)
        {
            var toAdd = await GetOneRecipeRecommendation(calorieTarget, toReturn, recipes);
            if (toAdd == null) continue;
            toReturn.Add(toAdd);
        }
        return toReturn;
    }

    private async Task<Recipe?> GetOneRecipeRecommendation(int calorieTarget, List<Recipe> recipesInMeal, List<Recipe> recipes)
    {
        int calorieCount = 0;
        calorieCount = recipesInMeal.Sum(r => r.Calories);

        var toReturn = recipes.First();
        recipes.Remove(toReturn);
        if (toReturn.Calories + calorieCount <= calorieTarget) return toReturn;
        return null;
    }
}