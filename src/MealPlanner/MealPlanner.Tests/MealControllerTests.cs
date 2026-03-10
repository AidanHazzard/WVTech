using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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
public class MealControllerTests
{
    private MealController _controller;
    private MealPlannerDBContext _context;

    private ClaimsPrincipal _user;

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

        // Fake logged-in user
        _user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "testuser")
        ],
        authenticationType: "TestAuth"));

        var registrationServiceMock = new Mock<IRegistrationService>();
        registrationServiceMock
            .Setup(r => r.FindUserByClaimAsync(_user))
            .ReturnsAsync(new User
            {
                Id = "user-1",
                FullName = "testuser"
            });

        var recipeRepoMock = new Mock<IRecipeRepository>();
        var mealRepoMock = new Mock<IMealRepository>();

        _controller = new MealController(
            registrationServiceMock.Object,
            recipeRepoMock.Object,
            mealRepoMock.Object,
            _context);

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

    // -----------------------------
    // ViewMeal tests
    // -----------------------------

    [Test]
    public async Task ViewMeal_WhenMealExistsForUser_ReturnsViewResult()
    {
        var meal = new Meal
        {
            Id = 1,
            UserId = "user-1",
            Recipes = new List<Recipe>()
        };

        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task ViewMeal_WhenMealExistsForUser_ReturnsMealAsModel()
    {
        var meal = new Meal
        {
            Id = 1,
            UserId = "user-1",
            Recipes = new List<Recipe>()
        };

        _context.Meals.Add(meal);
        _context.SaveChanges();

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
        var meal = new Meal
        {
            Id = 1,
            UserId = "user-1",
            Recipes =
            [
                new Recipe { Id = 1, Name = "Recipe 1", Directions = "D1" },
                new Recipe { Id = 2, Name = "Recipe 2", Directions = "D2" }
            ]
        };

        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1) as ViewResult;
        var model = (Meal)result!.Model!;

        Assert.That(model.Recipes, Is.Not.Null);
        Assert.That(model.Recipes.Count, Is.EqualTo(2));
        Assert.That(model.Recipes[0].Name, Is.EqualTo("Recipe 1"));
    }

    [Test]
    public async Task ViewMeal_WhenMealDoesNotExist_ReturnsNotFound()
    {
        var result = await _controller.ViewMeal(999);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task ViewMeal_WhenMealBelongsToDifferentUser_ReturnsNotFound()
    {
        var meal = new Meal
        {
            Id = 1,
            UserId = "other-user",
            Recipes = new List<Recipe>()
        };

        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task ViewMeal_WhenUserIsNull_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null }
        };

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }
}