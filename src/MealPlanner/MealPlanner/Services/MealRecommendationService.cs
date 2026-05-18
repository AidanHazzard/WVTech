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
    private IPantryService? _pantryService;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IUserDietaryRestrictionRepository dietaryRestrictionRepository,
        IUserFoodPreferenceRepository foodPreferenceRepository,
        IEnumerable<IRecommendationStream> streams,
        IPantryService? pantryService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _nutrionRepository = nutritionRepository;
        _dietaryRestrictionRepository = dietaryRestrictionRepository;
        _foodPreferenceRepository = foodPreferenceRepository;
        _streams = streams.ToList();
        _pantryService = pantryService;
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
        var pantryNames      = _pantryService?.GetPantryItems(userId)
            .Select(i => IngredientNameNormalizer.NormalizeKey(i.IngredientBase.Name))
            .ToHashSet() ?? [];
        return new UserRecommendationContext(restrictionNames, userVotes, votePercentages, upvoted, preferredTagIds, pantryNames);
    }

    private async Task<List<Recipe>> FetchSlotCandidatesAsync(RecommendationContext ctx)
    {
        var streamResults = _streams.Select(s => s.GetRankedCandidatesAsync(ctx).Result).ToList();

        // Streams have already dropped recipes excluded for this slot via the
        // ExcludedRecipeFilter; here we only de-duplicate across streams.
        var seenKeys = new HashSet<string>();
        var candidates = new List<Recipe>();
        foreach (var streamResult in streamResults)
        {
            foreach (var r in streamResult)
            {
                var key = RecipeKey.For(r);
                if (key == "uri:") continue;
                if (!seenKeys.Add(key)) continue;
                candidates.Add(r);
            }
        }
        return candidates;
    }

    public async Task<Recipe?> GetOneRecipeRecommendation(User user, DateTime date, IEnumerable<int> excludeRecipeIds)
    {
        var userCtx = await BuildUserContextAsync(user.Id);
        var excludeKeys = excludeRecipeIds.Select(id => $"id:{id}").ToHashSet();
        var mealCtx = new MealRecommendationContext(null, null, null, null, [], excludeKeys);
        var ctx = new RecommendationContext(userCtx, mealCtx);

        var candidates = await FetchSlotCandidatesAsync(ctx);
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
                pref.TagIds.ToHashSet(),
                new HashSet<string>(usedKeys));
            var ctx = new RecommendationContext(userCtx, mealCtx);

            var candidates = await FetchSlotCandidatesAsync(ctx);

            var recipes = MealComposer.Compose(
                candidates, calorieTarget, _MAX_RECIPES, proteinTarget, carbTarget, fatTarget);
            foreach (var r in recipes) usedKeys.Add(RecipeKey.For(r));

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
