using MealPlanner.DAL.Abstract;
using MealPlanner.Helpers;
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
    private IRecommendationStream _stream;
    private IExternalRecipeService? _externalRecipeService;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IUserDietaryRestrictionRepository dietaryRestrictionRepository,
        IUserFoodPreferenceRepository foodPreferenceRepository,
        IRecommendationStream stream,
        IExternalRecipeService? externalRecipeService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _nutrionRepository = nutritionRepository;
        _dietaryRestrictionRepository = dietaryRestrictionRepository;
        _foodPreferenceRepository = foodPreferenceRepository;
        _stream = stream;
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

    private async Task<UserRecommendationContext> BuildUserContextAsync(string userId)
    {
        var restrictionNames = await GetRestrictionNamesAsync(userId);
        var userVotes        = await _userRecipeRepository.GetUserVotesByUserIdAsync(userId);
        var votePercentages  = await _userRecipeRepository.GetAllVotePercentagesAsync();
        var upvoted          = await _userRecipeRepository.GetUserRecipesByVoteType(userId, UserVoteType.UpVote);
        await upvoted.LoadExternalRecipesAsync(_externalRecipeService);
        var preferredTagIds  = await _foodPreferenceRepository.GetFoodPreferenceTagIdsAsync(userId);
        return new UserRecommendationContext(restrictionNames, userVotes, votePercentages, upvoted, preferredTagIds);
    }

    public async Task<Recipe?> GetOneRecipeRecommendation(User user, DateTime date, IEnumerable<int> excludeRecipeIds)
    {
        var userCtx = await BuildUserContextAsync(user.Id);
        var mealCtx = new MealRecommendationContext(null, null, null, null, []);
        var ctx = new RecommendationContext(userCtx, mealCtx);
        var excludeIds = new HashSet<int>(excludeRecipeIds);
        return (await _stream.GetRankedCandidatesAsync(ctx))
            .FirstOrDefault(r => !excludeIds.Contains(r.Id));
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

            var mealCtx = new MealRecommendationContext(
                calorieTarget,
                proteinTarget,
                carbTarget,
                fatTarget,
                pref.TagIds.ToHashSet());
            var ctx = new RecommendationContext(userCtx, mealCtx);

            var candidates = (await _stream.GetRankedCandidatesAsync(ctx))
                .Where(r => !usedRecipeIds.Contains(r.Id));

            var recipes = MealComposer.PackUpToCalorieBudget(
                candidates, calorieTarget, _MAX_RECIPES, proteinTarget, carbTarget, fatTarget);
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
