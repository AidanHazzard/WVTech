using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services.Recommendation;
using MealPlanner.ViewModels;

namespace MealPlanner.Services;

public class MealRecommendationService : IMealRecommendationService
{
    private const int _MAX_RECIPES = 5;
    private IUserRecipeRepository _userRecipeRepository;
    private IUserNutritionPreferenceRepository _nutrionRepository;
    private IUserDietaryRestrictionRepository _dietaryRestrictionRepository;
    private IUserFoodPreferenceRepository _foodPreferenceRepository;
    private IReadOnlyList<IRecommendationStream> _streams;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IUserDietaryRestrictionRepository dietaryRestrictionRepository,
        IUserFoodPreferenceRepository foodPreferenceRepository,
        IEnumerable<IRecommendationStream> streams)
    {
        _userRecipeRepository = userRecipeRepository;
        _nutrionRepository = nutritionRepository;
        _dietaryRestrictionRepository = dietaryRestrictionRepository;
        _foodPreferenceRepository = foodPreferenceRepository;
        _streams = streams.ToList();
    }

    private async Task<HashSet<string>> GetRestrictionNamesAsync(string userId)
    {
        var restrictions = await _dietaryRestrictionRepository.GetByUserIdAsync(userId);
        return restrictions
            .Select(r => r.DietaryRestriction?.Name)
            .Where(n => n != null)
            .ToHashSet()!;
    }

    private async Task<UserRecommendationContext> BuildUserContextAsync(string userId)
    {
        var restrictionNames = await GetRestrictionNamesAsync(userId);
        var userVotes        = await _userRecipeRepository.GetUserVotesByUserIdAsync(userId);
        var votePercentages  = await _userRecipeRepository.GetAllVotePercentagesAsync();
        var upvoted          = await _userRecipeRepository.GetUserRecipesByVoteType(userId, UserVoteType.UpVote);
        var preferredTagIds  = await _foodPreferenceRepository.GetFoodPreferenceTagIdsAsync(userId);
        return new UserRecommendationContext(restrictionNames, userVotes, votePercentages, upvoted, preferredTagIds);
    }

    private static string RecipeKey(Recipe r) =>
        r.Id != 0 ? $"id:{r.Id}" : $"uri:{r.ExternalUri ?? string.Empty}";

    private async Task<List<Recipe>> FetchSlotCandidatesAsync(
        RecommendationContext ctx,
        HashSet<string> usedKeys)
    {
        var streamResults = await Task.WhenAll(_streams.Select(s => s.GetRankedCandidatesAsync(ctx)));

        var seenKeys = new HashSet<string>();
        var candidates = new List<Recipe>();
        foreach (var streamResult in streamResults)
        {
            foreach (var r in streamResult)
            {
                var key = RecipeKey(r);
                if (key == "uri:") continue;
                if (!seenKeys.Add(key)) continue;
                if (usedKeys.Contains(key)) continue;
                candidates.Add(r);
            }
        }
        return candidates;
    }

    public async Task<Recipe?> GetOneRecipeRecommendation(User user, DateTime date, IEnumerable<int> excludeRecipeIds)
    {
        var userCtx = await BuildUserContextAsync(user.Id);
        var mealCtx = new MealRecommendationContext(null, null, null, null, []);
        var ctx = new RecommendationContext(userCtx, mealCtx);

        var excludeIds = new HashSet<int>(excludeRecipeIds);
        var excludeKeys = excludeIds.Select(id => $"id:{id}").ToHashSet();

        var candidates = await FetchSlotCandidatesAsync(ctx, excludeKeys);
        return candidates.FirstOrDefault();
    }

    public async Task<List<Meal>> GetRecommendedMealsForUser(User user, DateTime mealDate, DayPlanConfigViewModel config)
    {
        var result = new List<Meal>();
        var preferences = config.MealPreferences.Any()
            ? new List<MealPreferenceViewModel>(config.MealPreferences)
            : Enumerable.Range(0, config.MealCount)
                .Select(_ => new MealPreferenceViewModel { Size = MealSize.Average })
                .ToList();

        var userCtx = await BuildUserContextAsync(user.Id);
        var nutritionPrefs = await _nutrionRepository.GetUsersNutritionPreferenceAsync(user.Id);
        var totalWeight = preferences.Sum(p => p.Size.Weight());

        var usedKeys = new HashSet<string>();
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

            var mealCtx = new MealRecommendationContext(
                calorieTarget,
                proteinTarget,
                carbTarget,
                fatTarget,
                pref.TagIds.ToHashSet());
            var ctx = new RecommendationContext(userCtx, mealCtx);

            var candidates = await FetchSlotCandidatesAsync(ctx, usedKeys);

            var recipes = MealComposer.PackUpToCalorieBudget(
                candidates, calorieTarget, _MAX_RECIPES, proteinTarget, carbTarget, fatTarget);
            foreach (var r in recipes) usedKeys.Add(RecipeKey(r));

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
