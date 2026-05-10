using System.Security.Claims;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class WVT170ControllerTests
{
    private MealController _controller = null!;
    private MealPlannerDBContext _context = null!;
    private ClaimsPrincipal _user = null!;

    private Mock<IMealRepository> _mealRepoMock = null!;
    private Mock<IRecipeRepository> _recipeRepoMock = null!;
    private Mock<IRegistrationService> _registrationServiceMock = null!;
    private Mock<IMealRecommendationService> _reccServiceMock = null!;
    private Mock<ITagRepository> _tagRepoMock = null!;

    private readonly User _appUser = new() { Id = "user-1", FullName = "testuser" };

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        _context = new MealPlannerDBContext(
            new DbContextOptionsBuilder<MealPlannerDBContext>().UseSqlite(connection).Options);
        _context.Database.EnsureCreated();

        _user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1"), new Claim(ClaimTypes.Name, "testuser")],
            "TestAuth"));

        _registrationServiceMock = new Mock<IRegistrationService>();
        _registrationServiceMock.Setup(r => r.FindUserByClaimAsync(_user)).ReturnsAsync(_appUser);

        _recipeRepoMock = new Mock<IRecipeRepository>();
        _mealRepoMock = new Mock<IMealRepository>();
        _reccServiceMock = new Mock<IMealRecommendationService>();
        _tagRepoMock = new Mock<ITagRepository>();
        _tagRepoMock.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync([]);

        _controller = new MealController(
            _registrationServiceMock.Object,
            _recipeRepoMock.Object,
            _mealRepoMock.Object,
            _context,
            _tagRepoMock.Object,
            _reccServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task RegenerateRecipe_WhenMealNotFound_ReturnsNotFound()
    {
        _mealRepoMock.Setup(r => r.ReadAsync(99)).ReturnsAsync((Meal)null!);

        var result = await _controller.RegenerateRecipe(99, 1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RegenerateRecipe_WhenMealBelongsToDifferentUser_ReturnsNotFound()
    {
        var meal = new Meal { Id = 1, UserId = "other-user", Recipes = [] };
        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);

        var result = await _controller.RegenerateRecipe(1, 10);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RegenerateRecipe_WhenNoAlternativeAvailable_ReturnsNoAlternativeJson()
    {
        var recipe = new Recipe { Id = 10, Name = "Old Recipe", Calories = 300 };
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = [recipe] };
        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);
        _reccServiceMock
            .Setup(s => s.GetOneRecipeRecommendation(_appUser, It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _controller.RegenerateRecipe(1, 10);

        var json = result as JsonResult;
        Assert.That(json, Is.Not.Null);
        var data = json!.Value as dynamic;
        Assert.That((bool)data!.noAlternative, Is.True);
    }

    [Test]
    public async Task RegenerateRecipe_WhenReplacementFound_ReturnsNewRecipeJson()
    {
        var oldRecipe = new Recipe { Id = 10, Name = "Old Recipe", Calories = 300 };
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = [oldRecipe] };
        var newRecipe = new Recipe { Id = 20, Name = "New Recipe", Calories = 400 };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);
        _mealRepoMock.Setup(r => r.CreateOrUpdate(meal)).Returns(meal);
        _reccServiceMock
            .Setup(s => s.GetOneRecipeRecommendation(_appUser, It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(newRecipe);

        var result = await _controller.RegenerateRecipe(1, 10);

        var json = result as JsonResult;
        Assert.That(json, Is.Not.Null);
        var data = json!.Value!;
        var newRecipeProp = data.GetType().GetProperty("newRecipe")!.GetValue(data)!;
        var id = (int)newRecipeProp.GetType().GetProperty("Id")!.GetValue(newRecipeProp)!;
        Assert.That(id, Is.EqualTo(20));
    }

    [Test]
    public async Task RegenerateRecipe_WhenReplacementFound_ReplacedRecipeIdReturnedInJson()
    {
        var oldRecipe = new Recipe { Id = 10, Name = "Old Recipe", Calories = 300 };
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = [oldRecipe] };
        var newRecipe = new Recipe { Id = 20, Name = "New Recipe", Calories = 400 };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);
        _mealRepoMock.Setup(r => r.CreateOrUpdate(meal)).Returns(meal);
        _reccServiceMock
            .Setup(s => s.GetOneRecipeRecommendation(_appUser, It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(newRecipe);

        var result = await _controller.RegenerateRecipe(1, 10);

        var json = result as JsonResult;
        var data = json!.Value!;
        var replacedId = (int)data.GetType().GetProperty("replacedRecipeId")!.GetValue(data)!;
        Assert.That(replacedId, Is.EqualTo(10));
    }

    [Test]
    public async Task RegenerateRecipe_WhenReplacementFound_CallsSaveChanges()
    {
        var oldRecipe = new Recipe { Id = 10, Name = "Old Recipe", Calories = 300 };
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = [oldRecipe] };
        var newRecipe = new Recipe { Id = 20, Name = "New Recipe", Calories = 400 };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);
        _mealRepoMock.Setup(r => r.CreateOrUpdate(meal)).Returns(meal);
        _reccServiceMock
            .Setup(s => s.GetOneRecipeRecommendation(_appUser, It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(newRecipe);

        await _controller.RegenerateRecipe(1, 10);

        _mealRepoMock.Verify(r => r.CreateOrUpdate(meal), Times.Once);
    }

    [Test]
    public async Task RegenerateRecipe_ExcludesAllCurrentRecipesFromRecommendation()
    {
        var recipe1 = new Recipe { Id = 10, Name = "Recipe 1", Calories = 200 };
        var recipe2 = new Recipe { Id = 11, Name = "Recipe 2", Calories = 200 };
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = [recipe1, recipe2] };
        var newRecipe = new Recipe { Id = 20, Name = "New Recipe", Calories = 300 };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);
        _mealRepoMock.Setup(r => r.CreateOrUpdate(meal)).Returns(meal);
        _reccServiceMock
            .Setup(s => s.GetOneRecipeRecommendation(_appUser, It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(newRecipe);

        await _controller.RegenerateRecipe(1, 10);

        _reccServiceMock.Verify(s => s.GetOneRecipeRecommendation(
            _appUser,
            It.IsAny<DateTime>(),
            It.Is<IEnumerable<int>>(ids => ids.Contains(10) && ids.Contains(11))),
            Times.Once);
    }
}

[TestFixture]
public class WVT170RecommendationServiceTests
{
    private Mock<IUserRecipeRepository> _userRecipeRepoMock = null!;
    private Mock<IRecipeRepository> _recipeRepoMock = null!;
    private Mock<IUserNutritionPreferenceRepository> _nutritionRepoMock = null!;
    private Mock<IUserDietaryRestrictionRepository> _dietaryRepoMock = null!;
    private MealRecommendationService _service = null!;

    private readonly User _user = new() { Id = "user-1" };

    [SetUp]
    public void SetUp()
    {
        _userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _nutritionRepoMock = new Mock<IUserNutritionPreferenceRepository>();
        _dietaryRepoMock = new Mock<IUserDietaryRestrictionRepository>();

        _userRecipeRepoMock.Setup(r => r.GetUserRecipesByVoteType(_user.Id, UserVoteType.UpVote)).ReturnsAsync([]);
        _userRecipeRepoMock.Setup(r => r.GetUserVotesByUserIdAsync(_user.Id)).ReturnsAsync([]);
        _userRecipeRepoMock.Setup(r => r.GetAllVotePercentagesAsync()).ReturnsAsync([]);
        _dietaryRepoMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync([]);
        _nutritionRepoMock.Setup(r => r.GetUsersNutritionPreferenceAsync(_user.Id))
            .ReturnsAsync(new UserNutritionPreference { UserId = _user.Id, CalorieTarget = 9999 });

        _service = new MealRecommendationService(
            _userRecipeRepoMock.Object,
            _recipeRepoMock.Object,
            _nutritionRepoMock.Object,
            _dietaryRepoMock.Object);
    }

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
}
