using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class MealRecommendationServiceTests
{
    private Mock<IUserRecipeRepository> _userRecipeRepoMock;
    private Mock<IRecipeRepository> _recipeRepoMock;
    private Mock<IUserNutritionPreferenceRepository> _nutritionRepoMock;
    private Mock<IUserDietaryRestrictionRepository> _dietaryRestrictionRepoMock;
    private MealRecommendationService _service;

    private readonly User _user = new() { Id = "user-1" };

    private static Tag VeganTag => new() { Id = 1, Name = "Vegan" };
    private static Tag GlutenFreeTag => new() { Id = 2, Name = "Gluten-Free" };

    [SetUp]
    public void SetUp()
    {
        _userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _nutritionRepoMock = new Mock<IUserNutritionPreferenceRepository>();
        _dietaryRestrictionRepoMock = new Mock<IUserDietaryRestrictionRepository>();

        _userRecipeRepoMock
            .Setup(r => r.GetUserRecipesByVoteType(_user.Id, UserVoteType.UpVote))
            .ReturnsAsync([]);
        _userRecipeRepoMock
            .Setup(r => r.GetUserRecipeVoteAsync(_user.Id, It.IsAny<int>()))
            .ReturnsAsync(UserVoteType.NoVote);
        _userRecipeRepoMock
            .Setup(r => r.GetRecipeVotePercentage(It.IsAny<int>()))
            .ReturnsAsync(0f);
        _userRecipeRepoMock
            .Setup(r => r.GetUserVotesByUserIdAsync(_user.Id))
            .ReturnsAsync(new Dictionary<int, UserVoteType>());
        _userRecipeRepoMock
            .Setup(r => r.GetAllVotePercentagesAsync())
            .ReturnsAsync(new Dictionary<int, float>());
        _nutritionRepoMock
            .Setup(r => r.GetUsersNutritionPreferenceAsync(_user.Id))
            .ReturnsAsync(new UserNutritionPreference { UserId = _user.Id, CalorieTarget = 9999 });
        _dietaryRestrictionRepoMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ReturnsAsync([]);

        _service = new MealRecommendationService(
            _userRecipeRepoMock.Object,
            _recipeRepoMock.Object,
            _nutritionRepoMock.Object,
            _dietaryRestrictionRepoMock.Object);
    }

    [Test]
    public async Task GetRecommendedMealsForUser_ExcludesDownvotedRecipes()
    {
        var downvoted = new Recipe { Id = 1, Name = "Downvoted", Calories = 300, Tags = [] };
        var allowed = new Recipe { Id = 2, Name = "Allowed", Calories = 300, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([downvoted, allowed]);
        _userRecipeRepoMock
            .Setup(r => r.GetUserVotesByUserIdAsync(_user.Id))
            .ReturnsAsync(new Dictionary<int, UserVoteType> { [1] = UserVoteType.DownVote });

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Not.Contain(downvoted));
        Assert.That(result[0].Recipes, Does.Contain(allowed));
    }

    [Test]
    public async Task GetRecommendedMealsForUser_WithNoDietaryRestrictions_IncludesAllCandidates()
    {
        var recipe = new Recipe { Id = 1, Name = "Any Recipe", Calories = 300, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Contain(recipe));
    }

    [Test]
    public async Task GetRecommendedMealsForUser_WithDietaryRestriction_IncludesMatchingRecipe()
    {
        var veganRecipe = new Recipe { Id = 1, Name = "Vegan Dish", Calories = 300, Tags = [VeganTag] };
        var restriction = new UserDietaryRestriction
        {
            UserId = _user.Id,
            DietaryRestriction = new DietaryRestriction { Id = 1, Name = "Vegan" }
        };
        _dietaryRestrictionRepoMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync([restriction]);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([veganRecipe]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Contain(veganRecipe));
    }

    [Test]
    public async Task GetRecommendedMealsForUser_WithDietaryRestriction_ExcludesNonMatchingRecipe()
    {
        var nonVeganRecipe = new Recipe { Id = 2, Name = "Beef Stew", Calories = 300, Tags = [] };
        var restriction = new UserDietaryRestriction
        {
            UserId = _user.Id,
            DietaryRestriction = new DietaryRestriction { Id = 1, Name = "Vegan" }
        };
        _dietaryRestrictionRepoMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync([restriction]);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([nonVeganRecipe]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Not.Contain(nonVeganRecipe));
    }

    [Test]
    public async Task GetRecommendedMealsForUser_WithMultipleMeals_DoesNotRepeatRecipesAcrossMeals()
    {
        var recipe1 = new Recipe { Id = 1, Name = "Recipe 1", Calories = 100, Tags = [] };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe 2", Calories = 100, Tags = [] };
        var recipe3 = new Recipe { Id = 3, Name = "Recipe 3", Calories = 100, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe1, recipe2, recipe3]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 2,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences =
            [
                new MealPreferenceViewModel { Size = MealSize.Average },
                new MealPreferenceViewModel { Size = MealSize.Average }
            ]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        var allAssigned = result.SelectMany(m => m.Recipes).ToList();
        var distinctIds = allAssigned.Select(r => r.Id).Distinct().ToList();
        Assert.That(allAssigned.Count, Is.EqualTo(distinctIds.Count), "Each recipe should appear in at most one meal");
    }

    [Test]
    public async Task GetRecommendedMealsForUser_WithMultipleRestrictions_RequiresAllTagsToMatch()
    {
        var veganOnly = new Recipe { Id = 1, Name = "Vegan Only", Calories = 300, Tags = [VeganTag] };
        var veganGlutenFree = new Recipe { Id = 2, Name = "Vegan GF", Calories = 300, Tags = [VeganTag, GlutenFreeTag] };

        var restrictions = new List<UserDietaryRestriction>
        {
            new() { UserId = _user.Id, DietaryRestriction = new DietaryRestriction { Id = 1, Name = "Vegan" } },
            new() { UserId = _user.Id, DietaryRestriction = new DietaryRestriction { Id = 2, Name = "Gluten-Free" } }
        };
        _dietaryRestrictionRepoMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(restrictions);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([veganOnly, veganGlutenFree]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Contain(veganGlutenFree));
        Assert.That(result[0].Recipes, Does.Not.Contain(veganOnly));
    }

    // --- Macro-target tests ---

    private static DayPlanConfigViewModel SingleMealConfig() => new()
    {
        MealCount = 1,
        SelectedMonth = DateTime.Today.Month,
        SelectedDay = DateTime.Today.Day,
        MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
    };

    private void SetNutritionPrefs(int calories = 9999, int? protein = null, int? carbs = null, int? fat = null) =>
        _nutritionRepoMock
            .Setup(r => r.GetUsersNutritionPreferenceAsync(_user.Id))
            .ReturnsAsync(new UserNutritionPreference
            {
                UserId = _user.Id,
                CalorieTarget = calories,
                ProteinTarget = protein,
                CarbTarget = carbs,
                FatTarget = fat
            });

    [Test]
    public async Task ProteinTarget_RecipeExceedsBudget_RecipeExcluded()
    {
        SetNutritionPrefs(protein: 20);
        var tooMuchProtein = new Recipe { Id = 1, Name = "High Protein", Calories = 100, Protein = 50, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([tooMuchProtein]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Not.Contain(tooMuchProtein));
    }

    [Test]
    public async Task CarbTarget_RecipeExceedsBudget_RecipeExcluded()
    {
        SetNutritionPrefs(carbs: 10);
        var tooManyCarbs = new Recipe { Id = 1, Name = "High Carb", Calories = 100, Carbs = 30, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([tooManyCarbs]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Not.Contain(tooManyCarbs));
    }

    [Test]
    public async Task FatTarget_RecipeExceedsBudget_RecipeExcluded()
    {
        SetNutritionPrefs(fat: 5);
        var tooMuchFat = new Recipe { Id = 1, Name = "High Fat", Calories = 100, Fat = 20, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([tooMuchFat]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Not.Contain(tooMuchFat));
    }

    [Test]
    public async Task AllMacroTargets_RecipeFitsAll_RecipeIncluded()
    {
        SetNutritionPrefs(calories: 500, protein: 50, carbs: 60, fat: 20);
        var balanced = new Recipe { Id = 1, Name = "Balanced", Calories = 200, Protein = 30, Carbs = 40, Fat = 10, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([balanced]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(balanced));
    }

    [Test]
    public async Task MacroStressTest_OneCal_200Protein_OneCarb_OneFat_OnlyFittingRecipeIncluded()
    {
        // Budget so extreme that only Recipe A (near-zero cal/carb/fat, high protein) survives all 4 checks.
        SetNutritionPrefs(calories: 1, protein: 200, carbs: 1, fat: 1);

        var fitsAll   = new Recipe { Id = 1, Name = "Fits",          Calories = 1,   Protein = 195, Carbs = 0, Fat = 1, Tags = [] };
        var failsCal  = new Recipe { Id = 2, Name = "Fails Calorie", Calories = 100, Protein = 10,  Carbs = 5, Fat = 5, Tags = [] };
        var failsCarb = new Recipe { Id = 3, Name = "Fails Carbs",   Calories = 0,   Protein = 5,   Carbs = 5, Fat = 0, Tags = [] };
        var failsFat  = new Recipe { Id = 4, Name = "Fails Fat",     Calories = 0,   Protein = 5,   Carbs = 0, Fat = 5, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([fitsAll, failsCal, failsCarb, failsFat]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(fitsAll),       "Recipe within all budgets should be recommended");
        Assert.That(result[0].Recipes, Does.Not.Contain(failsCal),  "Recipe exceeding calorie budget should be excluded");
        Assert.That(result[0].Recipes, Does.Not.Contain(failsCarb), "Recipe exceeding carb budget should be excluded");
        Assert.That(result[0].Recipes, Does.Not.Contain(failsFat),  "Recipe exceeding fat budget should be excluded");
    }

    [Test]
    public async Task OnlyProteinTargetSet_HighCarbAndFatNotBlocked()
    {
        SetNutritionPrefs(protein: 50); // no carb/fat targets
        var highCarbFat = new Recipe { Id = 1, Name = "High Carb Fat", Calories = 100, Protein = 30, Carbs = 999, Fat = 999, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([highCarbFat]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(highCarbFat), "Carbs/fat should be ignored when no target is set for them");
    }

    [Test]
    public async Task RunningProteinTotal_SecondRecipeExcludedWhenCombinedExceedsBudget()
    {
        SetNutritionPrefs(protein: 50);
        var recipeA = new Recipe { Id = 1, Name = "Recipe A", Calories = 100, Protein = 30, Tags = [] };
        var recipeB = new Recipe { Id = 2, Name = "Recipe B", Calories = 100, Protein = 30, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipeA, recipeB]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(recipeA),     "First recipe (30g) fits the 50g budget");
        Assert.That(result[0].Recipes, Does.Not.Contain(recipeB), "Second recipe (30g+30g=60g) exceeds the 50g budget");
    }

    [Test]
    public async Task MacroTargets_ScaleProportionallyToMealSize()
    {
        // Daily protein = 60g. Small weight=0.5, Large weight=1.5, totalWeight=2.0.
        // Small budget: round(0.5/2.0 * 60) = 15g  →  20g protein recipe is excluded
        // Large budget: round(1.5/2.0 * 60) = 45g  →  20g protein recipe is included
        SetNutritionPrefs(protein: 60);
        var recipe = new Recipe { Id = 1, Name = "Moderate Protein", Calories = 100, Protein = 20, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe]);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 2,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences =
            [
                new MealPreferenceViewModel { Size = MealSize.Small },  // protein budget = 15g
                new MealPreferenceViewModel { Size = MealSize.Large }   // protein budget = 45g
            ]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Not.Contain(recipe), "Small meal (15g budget) should exclude 20g protein recipe");
        Assert.That(result[1].Recipes, Does.Contain(recipe),     "Large meal (45g budget) should include 20g protein recipe");
    }

    // --- GetOneRecipeRecommendation ---

    [Test]
    public async Task GetOneRecipeRecommendation_WhenNoRecipesExist_ReturnsNull()
    {
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, []);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetOneRecipeRecommendation_WhenRecipesAvailable_ReturnsOneRecipe()
    {
        var recipe = new Recipe { Id = 1, Name = "Pasta", Calories = 400, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, []);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetOneRecipeRecommendation_ExcludesRecipesInExcludeList()
    {
        var recipe1 = new Recipe { Id = 1, Name = "Pasta", Calories = 400, Tags = [] };
        var recipe2 = new Recipe { Id = 2, Name = "Salad", Calories = 200, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe1, recipe2]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, [1]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task GetOneRecipeRecommendation_WhenAllRecipesExcluded_ReturnsNull()
    {
        var recipe = new Recipe { Id = 1, Name = "Pasta", Calories = 400, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, [1]);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetOneRecipeRecommendation_PrefersUpvotedRecipes()
    {
        var upvotedRecipe = new Recipe { Id = 2, Name = "Upvoted Pasta", Calories = 400, Tags = [] };
        var normalRecipe = new Recipe { Id = 1, Name = "Normal Salad", Calories = 200, Tags = [] };

        _userRecipeRepoMock
            .Setup(r => r.GetUserRecipesByVoteType(_user.Id, UserVoteType.UpVote))
            .ReturnsAsync([upvotedRecipe]);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([normalRecipe, upvotedRecipe]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, []);

        Assert.That(result!.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task GetOneRecipeRecommendation_HonorsDietaryRestrictions()
    {
        var veganRecipe   = new Recipe { Id = 1, Name = "Vegan Bowl", Calories = 400, Tags = [VeganTag] };
        var regularRecipe = new Recipe { Id = 2, Name = "Beef Stew",  Calories = 400, Tags = [] };
        var restriction = new UserDietaryRestriction
        {
            UserId = _user.Id,
            DietaryRestriction = new DietaryRestriction { Id = 1, Name = "Vegan" }
        };
        _dietaryRestrictionRepoMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync([restriction]);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([regularRecipe, veganRecipe]);

        var result = await _service.GetOneRecipeRecommendation(_user, DateTime.Today, []);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1), "Should return only the recipe matching the dietary restriction");
    }

    // --- Tag preference and ordering tests ---

    private static Tag ItalianTag => new() { Id = 3, Name = "Italian" };

    [Test]
    public async Task TagPreference_RecipeWithMatchingTagSelectedOverRecipeWithout()
    {
        // Budget allows only 1 of the two 100-cal recipes; the one matching the preferred tag should win.
        SetNutritionPrefs(calories: 150);
        var withTag    = new Recipe { Id = 1, Name = "Pasta",  Calories = 100, Tags = [ItalianTag] };
        var withoutTag = new Recipe { Id = 2, Name = "Salad",  Calories = 100, Tags = [] };
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([withoutTag, withTag]); // withoutTag listed first

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average, TagIds = [3] }]
        };

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, config);

        Assert.That(result[0].Recipes, Does.Contain(withTag),        "Recipe matching preferred tag should be selected");
        Assert.That(result[0].Recipes, Does.Not.Contain(withoutTag), "Recipe without preferred tag should be displaced");
    }

    [Test]
    public async Task UpvotedRecipe_OutranksHighCommunityVotePercentageNonUpvotedRecipe()
    {
        // Budget allows only 1 recipe; upvoted should win even against a 90% community-vote recipe.
        SetNutritionPrefs(calories: 150);
        var upvoted = new Recipe { Id = 1, Name = "Upvoted",  Calories = 100, Tags = [] };
        var popular = new Recipe { Id = 2, Name = "Popular",  Calories = 100, Tags = [] };

        _userRecipeRepoMock
            .Setup(r => r.GetUserRecipesByVoteType(_user.Id, UserVoteType.UpVote))
            .ReturnsAsync([upvoted]);
        _userRecipeRepoMock
            .Setup(r => r.GetAllVotePercentagesAsync())
            .ReturnsAsync(new Dictionary<int, float> { [2] = 0.9f });
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([popular, upvoted]); // popular listed first

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(upvoted),    "Upvoted recipe should be preferred over high vote% recipe");
        Assert.That(result[0].Recipes, Does.Not.Contain(popular), "High vote% recipe should be displaced by upvoted recipe");
    }

    [Test]
    public async Task VotePercentage_HigherVotePercentageNonUpvotedRecipeSelectedFirst()
    {
        // Budget allows only 1 recipe; among non-upvoted candidates, the one with the higher vote% wins.
        SetNutritionPrefs(calories: 150);
        var highVote = new Recipe { Id = 1, Name = "Popular",   Calories = 100, Tags = [] };
        var lowVote  = new Recipe { Id = 2, Name = "Unpopular", Calories = 100, Tags = [] };

        _userRecipeRepoMock
            .Setup(r => r.GetAllVotePercentagesAsync())
            .ReturnsAsync(new Dictionary<int, float> { [1] = 0.7f, [2] = 0.3f });
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([lowVote, highVote]); // lowVote listed first

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes, Does.Contain(highVote),  "Recipe with higher vote% should be selected");
        Assert.That(result[0].Recipes, Does.Not.Contain(lowVote), "Recipe with lower vote% should be displaced");
    }

    // --- Known bug: greedy macro underfill ---

    [Ignore("Known bug: greedy macro-fill commits to the high-protein upvoted recipe first, exhausting the protein budget and blocking lower-protein recipes that would otherwise fit")]
    [Test]
    public async Task GreedyMacroFill_HighProteinUpvotedRecipeExhaustsProteinBudget_SmallerRecipesStillAdded()
    {
        // bigProtein (upvoted, 30g) is selected first and fills the entire 30g protein budget.
        // smallProA and smallProB (5g each) are then excluded: 30+5=35 > 30.
        // A smarter algorithm would skip bigProtein in favour of fitting more recipes under the budget.
        SetNutritionPrefs(calories: 9999, protein: 30);
        var bigProtein = new Recipe { Id = 1, Name = "Protein Shake", Calories = 100, Protein = 30, Tags = [] };
        var smallProA  = new Recipe { Id = 2, Name = "Light Snack A", Calories = 100, Protein = 5,  Tags = [] };
        var smallProB  = new Recipe { Id = 3, Name = "Light Snack B", Calories = 100, Protein = 5,  Tags = [] };

        _userRecipeRepoMock
            .Setup(r => r.GetUserRecipesByVoteType(_user.Id, UserVoteType.UpVote))
            .ReturnsAsync([bigProtein]);
        _recipeRepoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([bigProtein, smallProA, smallProB]);

        var result = await _service.GetRecommendedMealsForUser(_user, DateTime.Today, SingleMealConfig());

        Assert.That(result[0].Recipes.Count, Is.GreaterThanOrEqualTo(2),
            "Should not let one high-macro recipe block all remaining candidates from the meal");
    }
}
