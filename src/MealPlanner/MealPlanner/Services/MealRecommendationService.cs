using MealPlanner.DAL.Abstract;
using MealPlanner.Helpers;
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
    private IUserDietaryRestrictionRepository _dietaryRestrictionRepository;
    private IExternalRecipeService? _externalRecipeService;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IRecipeRepository recipeRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IUserDietaryRestrictionRepository dietaryRestrictionRepository,
        IExternalRecipeService? externalRecipeService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _recipeRepository = recipeRepository;
        _nutrionRepository = nutritionRepository;
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

    // Yields candidates in priority order: upvoted first, then non-downvoted sorted by community
    // vote percentage. Excludes any recipe whose ID is in excludeIds.
    private static IEnumerable<Recipe> OrderedCandidates(
        IEnumerable<Recipe> upvoted,
        IEnumerable<Recipe> allWithTags,
        Dictionary<int, UserVoteType> userVotes,
        Dictionary<int, float> votePercentages,
        HashSet<int> excludeIds)
    {
        var upvotedList = upvoted.Where(r => !excludeIds.Contains(r.Id)).ToList();
        var upvotedIds = upvotedList.Select(r => r.Id).ToHashSet();

        foreach (var r in upvotedList) yield return r;

        foreach (var r in allWithTags
            .Where(r => userVotes.GetValueOrDefault(r.Id, UserVoteType.NoVote) != UserVoteType.DownVote
                     && !upvotedIds.Contains(r.Id)
                     && !excludeIds.Contains(r.Id))
            .OrderByDescending(r => votePercentages.GetValueOrDefault(r.Id, 0f)))
        {
            yield return r;
        }
    }

    // Applies dietary restriction and tag-preference filters lazily, then greedily picks
    // up to _MAX_RECIPES that fit within the calorie and macro budgets.
    private static List<Recipe> SelectFromCandidates(
        IEnumerable<Recipe> candidates,
        int calorieTarget,
        HashSet<string> restrictionNames,
        IReadOnlyList<int> preferredTagIds,
        int? proteinTarget = null,
        int? carbTarget = null,
        int? fatTarget = null)
    {
        IEnumerable<Recipe> pipeline = candidates;

        if (restrictionNames.Count > 0)
            pipeline = pipeline.Where(r => restrictionNames.All(name => r.Tags.Any(t => t.Name == name)));

        // Sort by number of matching tags descending; within same count, upvote/vote order is preserved
        if (preferredTagIds.Count > 0)
            pipeline = pipeline.OrderByDescending(r => r.Tags.Count(t => preferredTagIds.Contains(t.Id)));

        var recipes = new List<Recipe>();
        int runningCalories = 0, runningProtein = 0, runningCarbs = 0, runningFat = 0;
        foreach (var recipe in pipeline)
        {
            if (recipes.Count >= _MAX_RECIPES) break;
            if (recipe.Calories + runningCalories <= calorieTarget
                && (!proteinTarget.HasValue || recipe.Protein + runningProtein <= proteinTarget.Value)
                && (!carbTarget.HasValue   || recipe.Carbs   + runningCarbs   <= carbTarget.Value)
                && (!fatTarget.HasValue    || recipe.Fat     + runningFat     <= fatTarget.Value))
            {
                recipes.Add(recipe);
                runningCalories += recipe.Calories;
                runningProtein  += recipe.Protein;
                runningCarbs    += recipe.Carbs;
                runningFat      += recipe.Fat;
            }
        }
        return recipes;
    }

    public async Task<Recipe?> GetOneRecipeRecommendation(User user, DateTime date, IEnumerable<int> excludeRecipeIds)
    {
        var restrictionNames = await GetRestrictionNamesAsync(user.Id);
        var userVotes = await _userRecipeRepository.GetUserVotesByUserIdAsync(user.Id);
        var votePercentages = await _userRecipeRepository.GetAllVotePercentagesAsync();
        var upvoted = await _userRecipeRepository.GetUserRecipesByVoteType(user.Id, UserVoteType.UpVote);
        var allWithTags = await _recipeRepository.GetAllWithTagsAsync();
        await upvoted.LoadExternalRecipesAsync(_externalRecipeService);
        await allWithTags.LoadExternalRecipesAsync(_externalRecipeService);

        var excludeIds = new HashSet<int>(excludeRecipeIds);
        var candidates = OrderedCandidates(upvoted, allWithTags, userVotes, votePercentages, excludeIds);
        return SelectFromCandidates(candidates, int.MaxValue, restrictionNames, []).FirstOrDefault();
    }

    public async Task<List<Meal>> GetRecommendedMealsForUser(User user, DateTime mealDate, DayPlanConfigViewModel config)
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
        await upvoted.LoadExternalRecipesAsync(_externalRecipeService);
        await allWithTags.LoadExternalRecipesAsync(_externalRecipeService);

        var nutritionPrefs = await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id);
        var totalWeight = preferences.Sum(p => p.Size.Weight());

        var usedRecipeIds = new HashSet<int>();
        int mealIndex = 0;
        foreach (var pref in preferences)
        {
            double weight = pref.Size.Weight() / totalWeight;
            var calorieTarget = nutritionPrefs?.CalorieTarget.HasValue == true
                ? (int)Math.Round(weight * nutritionPrefs.CalorieTarget.Value)
                : pref.Size.Calories();
            var proteinTarget = nutritionPrefs?.ProteinTarget.HasValue == true
                ? (int)Math.Round(weight * nutritionPrefs.ProteinTarget.Value)
                : (int?)null;
            var carbTarget = nutritionPrefs?.CarbTarget.HasValue == true
                ? (int)Math.Round(weight * nutritionPrefs.CarbTarget.Value)
                : (int?)null;
            var fatTarget = nutritionPrefs?.FatTarget.HasValue == true
                ? (int)Math.Round(weight * nutritionPrefs.FatTarget.Value)
                : (int?)null;

            var candidates = OrderedCandidates(upvoted, allWithTags, userVotes, votePercentages, usedRecipeIds);
            var recipes = SelectFromCandidates(candidates, calorieTarget, restrictionNames, pref.TagIds, proteinTarget, carbTarget, fatTarget);
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
