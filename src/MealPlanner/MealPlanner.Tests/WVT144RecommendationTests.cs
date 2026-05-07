using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class WVT144RecommendationTests
{
    private Mock<IUserRecipeRepository> _userRecipeRepoMock;
    private Mock<IRecipeRepository> _recipeRepoMock;
    private Mock<IUserNutritionPreferenceRepository> _nutritionRepoMock;
    private Mock<IMealRepository> _mealRepoMock;
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
        _mealRepoMock = new Mock<IMealRepository>();
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
        _mealRepoMock
            .Setup(r => r.GetUserMealsByDateAsync(_user, It.IsAny<DateTime>()))
            .ReturnsAsync([]);
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
}
