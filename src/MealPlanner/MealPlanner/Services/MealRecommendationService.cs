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
    private IUserDietaryRestrictionRepository _dietaryRestrictionRepository;
    private IExternalRecipeService? _externalRecipeService;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IRecipeRepository recipeRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IMealRepository mealRepository,
        IUserDietaryRestrictionRepository dietaryRestrictionRepository,
        IExternalRecipeService? externalRecipeService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _recipeRepository = recipeRepository;
        _nutrionRepository = nutritionRepository;
        _mealRepository = mealRepository;
        _dietaryRestrictionRepository = dietaryRestrictionRepository;
        _externalRecipeService = externalRecipeService;
    }

    private async Task<HashSet<string>> GetRestrictionNamesAsync(string userId)
    {
        var restrictions = await _dietaryRestrictionRepository.GetByUserIdAsync(userId);
        return restrictions
            .Select(r => r.DietaryRestriction?.Name)
            .Where(n => n != null)
            .ToHashSet()!;
    }

    private static List<Recipe> ApplyDietaryFilter(List<Recipe> candidates, HashSet<string> restrictionNames)
    {
        if (restrictionNames.Count == 0) return candidates;
        return candidates
            .Where(r => restrictionNames.All(name => r.Tags.Any(t => t.Name == name)))
            .ToList();
    }

    public async Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate)
    {
        var restrictionNames = await GetRestrictionNamesAsync(user.Id);
        var existingRecipes = (await _mealRepository.GetUserMealsByDateAsync(user, mealDate)).SelectMany(m => m.Recipes);
        var recipes = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        recipes = recipes.Where(r => !existingRecipes.Contains(r)).ToList();

        var allWithTags = await _recipeRepository.GetAllWithTagsAsync();
        var userVotes = await _userRecipeRepository.GetUserVotesByUserIdAsync(user.Id);
        var votePercentages = await _userRecipeRepository.GetAllVotePercentagesAsync();
        var existingIds = existingRecipes.Select(r => r.Id).ToHashSet();
        var recipeIds = recipes.Select(r => r.Id).ToHashSet();
        var rest = allWithTags
            .Where(r => userVotes.GetValueOrDefault(r.Id, UserVoteType.NoVote) != UserVoteType.DownVote
                     && !existingIds.Contains(r.Id)
                     && !recipeIds.Contains(r.Id))
            .OrderByDescending(r => votePercentages.GetValueOrDefault(r.Id, 0f))
            .ToList();

        recipes.AddRange(rest);
        await LoadExternalRecipesAsync(recipes);
        recipes = ApplyDietaryFilter(recipes, restrictionNames);

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

    private async Task LoadExternalRecipesAsync(List<Recipe> recipes)
    {
        if (_externalRecipeService == null) return;

        for(int i = 0; i < recipes.Count; i++)
        {
            var r = recipes[i];
            if (r.ExternalUri.IsNullOrEmpty()) continue;
            try
            {
                r = await _externalRecipeService.GetExternalRecipeByURI(r.ExternalUri!);
            }
            catch
            {
                if (r != null) recipes.Remove(r);
            }
        }
    }

    private async Task<(Recipe? Recommendation, int mealCalories)> GetOneRecipeRecommendation(int calorieTarget, List<Recipe> recipes, int runningMealCalorieCount)
    {
        var toReturn = recipes.First();
        recipes.Remove(toReturn);

        if (toReturn != null && toReturn.Calories + runningMealCalorieCount <= calorieTarget)
        {
            return (toReturn, runningMealCalorieCount + toReturn.Calories);
        }
        return (null, runningMealCalorieCount);
    }

    public async Task<List<Meal>> GetRecommendedDayPlanForUser(User user, DateTime mealDate, DayPlanConfigViewModel config)
    {
        var result = new List<Meal>();
        var restrictionNames = await GetRestrictionNamesAsync(user.Id);
        var preferences = config.MealPreferences.Any()
            ? new List<MealPreferenceViewModel>(config.MealPreferences)
            : Enumerable.Range(0, config.MealCount)
                .Select(_ => new MealPreferenceViewModel { Size = MealSize.Average })
                .ToList();

        var userVotes = await _userRecipeRepository.GetUserVotesByUserIdAsync(user.Id);
        var votePercentages = await _userRecipeRepository.GetAllVotePercentagesAsync();
        var upvoted = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        var allWithTags = await _recipeRepository.GetAllWithTagsAsync();

        var usedRecipeIds = new HashSet<int>();
        int mealIndex = 0;
        foreach (var pref in preferences)
        {
            var calorieTarget = pref.Size.Calories();

            var upvotedIds = upvoted.Select(r => r.Id).ToHashSet();
            var rest = allWithTags
                .Where(r => userVotes.GetValueOrDefault(r.Id, UserVoteType.NoVote) != UserVoteType.DownVote
                         && !upvotedIds.Contains(r.Id))
                .OrderByDescending(r => votePercentages.GetValueOrDefault(r.Id, 0f))
                .ToList();

            var candidates = upvoted.Concat(rest)
                .Where(r => !usedRecipeIds.Contains(r.Id))
                .ToList();
            candidates = ApplyDietaryFilter(candidates, restrictionNames);

            if (pref.TagIds.Any())
            {
                var matched = candidates.Where(r => r.Tags.Any(t => pref.TagIds.Contains(t.Id))).ToList();
                var unmatched = candidates.Where(r => r.Tags.All(t => !pref.TagIds.Contains(t.Id))).ToList();
                candidates = matched.Concat(unmatched).ToList();
            }

            var recipes = new List<Recipe>();
            int running = 0;
            foreach (var recipe in candidates)
            {
                if (recipes.Count >= _MAX_RECIPES) break;
                if (recipe.Calories + running <= calorieTarget)
                {
                    recipes.Add(recipe);
                    running += recipe.Calories;
                }
            }

            usedRecipeIds.UnionWith(recipes.Select(r => r.Id));

            result.Add(new Meal
            {
                User = user,
                UserId = user.Id,
                Title = !string.IsNullOrWhiteSpace(pref.Title) ? pref.Title : $"Meal {++mealIndex}",
                StartTime = mealDate,
                Recipes = recipes
            });
        }

        return result;
    }
}