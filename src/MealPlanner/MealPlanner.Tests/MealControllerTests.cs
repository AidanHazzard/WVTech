using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class MealControllerTests
{
    private MealController _controller;
    private MealPlannerDBContext _context;
    private ClaimsPrincipal _user;

    private Mock<IMealRepository> _mealRepoMock;
    private Mock<IRecipeRepository> _recipeRepoMock;
    private Mock<IRegistrationService> _registrationServiceMock;
    private Mock<IMealRecommendationService> _reccServiceMock;

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection)
            .Options;

        _context = new MealPlannerDBContext(contextOptions);
        _context.Database.EnsureCreated();

        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Name, "testuser")
            },
            "TestAuth"));

        _registrationServiceMock = new Mock<IRegistrationService>();
        _registrationServiceMock.Setup(r => r.FindUserByClaimAsync(_user))
            .ReturnsAsync(new User { Id = "user-1", FullName = "testuser" });

        _recipeRepoMock = new Mock<IRecipeRepository>();
        _mealRepoMock = new Mock<IMealRepository>();
        _reccServiceMock = new Mock<IMealRecommendationService>();

        _controller = new MealController(
            _registrationServiceMock.Object,
            _recipeRepoMock.Object,
            _mealRepoMock.Object,
            _context,
            _reccServiceMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _user }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task ViewMeal_WhenMealExistsForUser_ReturnsViewResult()
    {
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = new List<Recipe>() };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task ViewMeal_WhenMealExistsForUser_ReturnsMealAsModel()
    {
        var meal = new Meal { Id = 1, UserId = "user-1", Recipes = new List<Recipe>() };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal)).Returns(Task.CompletedTask);

        var result = await _controller.ViewMeal(1) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Model, Is.TypeOf<Meal>());

        var model = (Meal)result.Model!;
        Assert.That(model.Id, Is.EqualTo(1));
        Assert.That(model.UserId, Is.EqualTo("user-1"));
    }

    [Test]
    public async Task ViewMeal_IncludesRecipes()
    {
        var recipe1 = new Recipe { Id = 1, Name = "Recipe 1", Directions = "D1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe 2", Directions = "D2" };
        var meal = new Meal { Id = 1, UserId = "user-1" };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);
        _mealRepoMock.Setup(r => r.LoadRecipesAsync(meal))
            .Callback(() => meal.Recipes = new List<Recipe> { recipe1, recipe2 })
            .Returns(Task.CompletedTask);

        var result = await _controller.ViewMeal(1) as ViewResult;
        var model = (Meal)result!.Model!;

        Assert.That(model.Recipes, Is.Not.Null);
        Assert.That(model.Recipes.Count, Is.EqualTo(2));
        Assert.That(model.Recipes[0].Name, Is.EqualTo("Recipe 1"));
        Assert.That(model.Recipes[1].Name, Is.EqualTo("Recipe 2"));
    }

    [Test]
    public async Task ViewMeal_WhenMealDoesNotExist_ReturnsNotFound()
    {
        _mealRepoMock.Setup(r => r.ReadAsync(999)).ReturnsAsync((Meal)null);

        var result = await _controller.ViewMeal(999);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task ViewMeal_WhenMealBelongsToDifferentUser_ReturnsNotFound()
    {
        var meal = new Meal { Id = 1, UserId = "other-user", Recipes = new List<Recipe>() };

        _mealRepoMock.Setup(r => r.ReadAsync(1)).ReturnsAsync(meal);

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditMeal_Get_WhenMealExistsForUser_ReturnsViewResult()
    {
        var meal = new Meal
        {
            Id = 10,
            UserId = "user-1",
            Recipes = new List<Recipe>()
        };

        _mealRepoMock.Setup(r => r.ReadAsync(10)).ReturnsAsync(meal);

        var result = await _controller.EditMeal(10);

        Assert.That(result, Is.TypeOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.TypeOf<EditMealViewModel>());
        var model = (EditMealViewModel)viewResult.Model!;
        Assert.That(model.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task EditMeal_Get_WhenMealDoesNotExist_ReturnsNotFound()
    {
        _mealRepoMock.Setup(r => r.ReadAsync(999)).ReturnsAsync((Meal)null);

        var result = await _controller.EditMeal(999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GenerateMeal_RedirectsToViewMeal_ForNewMeal()
    {
        // Arrange
        CreateMealViewModel vm = new CreateMealViewModel()
        {
            Title = "Test",
            SelectedMonth = 1,
            SelectedDay = 1
        };

        Meal meal = new Meal()
        {
            Id = 100,
            Title = vm.Title,
            StartTime = new DateTime(DateTime.Today.Year, vm.SelectedMonth, vm.SelectedDay)
        };

        _reccServiceMock.Setup(s => s.GetRecommendedRecipesForUser(It.IsAny<User>(), It.IsAny<DateTime>()))
            .ReturnsAsync([new Recipe()]);
        _mealRepoMock.Setup(r => r.CreateOrUpdate(It.IsAny<Meal>())).Returns(meal);

        // Act
        var result = await _controller.GenerateMeal(vm);

        // Assert
        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(redirectResult!.ActionName, Is.EqualTo("ViewMeal"));
            Assert.That(redirectResult.RouteValues!["id"], Is.EqualTo(meal.Id));
        }
    }
}