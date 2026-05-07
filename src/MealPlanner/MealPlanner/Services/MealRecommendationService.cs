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

    // Builds an ordered candidate list: upvoted recipes first, then all others (non-downvoted)
    // sorted by community vote percentage, excluding any IDs in excludeIds.
    private static List<Recipe> BuildOrderedCandidates(
        IEnumerable<Recipe> upvoted,
        IEnumerable<Recipe> allWithTags,
        Dictionary<int, UserVoteType> userVotes,
        Dictionary<int, float> votePercentages,
        HashSet<int> excludeIds)
    {
        var upvotedFiltered = upvoted.Where(r => !excludeIds.Contains(r.Id)).ToList();
        var upvotedIds = upvotedFiltered.Select(r => r.Id).ToHashSet();
        var rest = allWithTags
            .Where(r => userVotes.GetValueOrDefault(r.Id, UserVoteType.NoVote) != UserVoteType.DownVote
                     && !upvotedIds.Contains(r.Id)
                     && !excludeIds.Contains(r.Id))
            .OrderByDescending(r => votePercentages.GetValueOrDefault(r.Id, 0f))
            .ToList();
        return upvotedFiltered.Concat(rest).ToList();
    }

    // Reorders candidates so tag-preferred recipes come first, then greedily picks
    // up to _MAX_RECIPES that fit within the calorie budget.
    private static List<Recipe> SelectRecipesFromCandidates(
        List<Recipe> candidates,
        int calorieTarget,
        IReadOnlyList<int> preferredTagIds)
    {
        if (preferredTagIds.Count > 0)
        {
            var matched = candidates.Where(r => r.Tags.Any(t => preferredTagIds.Contains(t.Id))).ToList();
            var unmatched = candidates.Where(r => r.Tags.All(t => !preferredTagIds.Contains(t.Id))).ToList();
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
        return recipes;
    }

    public async Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate)
    {
        var restrictionNames = await GetRestrictionNamesAsync(user.Id);
        var existingRecipes = (await _mealRepository.GetUserMealsByDateAsync(user, mealDate))
            .SelectMany(m => m.Recipes).ToList();
        var existingIds = existingRecipes.Select(r => r.Id).ToHashSet();

        var upvoted = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        var allWithTags = await _recipeRepository.GetAllWithTagsAsync();
        var userVotes = await _userRecipeRepository.GetUserVotesByUserIdAsync(user.Id);
        var votePercentages = await _userRecipeRepository.GetAllVotePercentagesAsync();

        var candidates = BuildOrderedCandidates(upvoted, allWithTags, userVotes, votePercentages, existingIds);
        await LoadExternalRecipesAsync(candidates);
        candidates = ApplyDietaryFilter(candidates, restrictionNames);

        var calorieTarget = (await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id))?.CalorieTarget ?? int.MaxValue;
        calorieTarget -= existingRecipes.Sum(r => r.Calories);

        return SelectRecipesFromCandidates(candidates, calorieTarget, []);
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

        var dailyCalorieTarget = (await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id))?.CalorieTarget;
        var totalWeight = preferences.Sum(p => p.Size.Weight());

        var usedRecipeIds = new HashSet<int>();
        int mealIndex = 0;
        foreach (var pref in preferences)
        {
            var calorieTarget = dailyCalorieTarget.HasValue
                ? (int)Math.Round(pref.Size.Weight() / totalWeight * dailyCalorieTarget.Value)
                : pref.Size.Calories();

            var candidates = BuildOrderedCandidates(upvoted, allWithTags, userVotes, votePercentages, usedRecipeIds);
            candidates = ApplyDietaryFilter(candidates, restrictionNames);

            var recipes = SelectRecipesFromCandidates(candidates, calorieTarget, pref.TagIds);
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
