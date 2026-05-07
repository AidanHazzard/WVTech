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
    // up to _MAX_RECIPES that fit within the calorie budget.
    private static List<Recipe> SelectFromCandidates(
        IEnumerable<Recipe> candidates,
        int calorieTarget,
        HashSet<string> restrictionNames,
        IReadOnlyList<int> preferredTagIds)
    {
        IEnumerable<Recipe> pipeline = candidates;

        if (restrictionNames.Count > 0)
            pipeline = pipeline.Where(r => restrictionNames.All(name => r.Tags.Any(t => t.Name == name)));

        // Stable sort: tag-matched recipes first, unmatched after (preserves priority order within each group)
        if (preferredTagIds.Count > 0)
            pipeline = pipeline.OrderBy(r => r.Tags.Any(t => preferredTagIds.Contains(t.Id)) ? 0 : 1);

        var recipes = new List<Recipe>();
        int running = 0;
        foreach (var recipe in pipeline)
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

    private async Task LoadExternalRecipesAsync(List<Recipe> recipes)
    {
        if (_externalRecipeService == null) return;

        for (int i = 0; i < recipes.Count; i++)
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
        await LoadExternalRecipesAsync(upvoted);
        await LoadExternalRecipesAsync(allWithTags);

        var dailyCalorieTarget = (await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id))?.CalorieTarget;
        var totalWeight = preferences.Sum(p => p.Size.Weight());

        var usedRecipeIds = new HashSet<int>();
        int mealIndex = 0;
        foreach (var pref in preferences)
        {
            var calorieTarget = dailyCalorieTarget.HasValue
                ? (int)Math.Round(pref.Size.Weight() / totalWeight * dailyCalorieTarget.Value)
                : pref.Size.Calories();

            var candidates = OrderedCandidates(upvoted, allWithTags, userVotes, votePercentages, usedRecipeIds);
            var recipes = SelectFromCandidates(candidates, calorieTarget, restrictionNames, pref.TagIds);
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
