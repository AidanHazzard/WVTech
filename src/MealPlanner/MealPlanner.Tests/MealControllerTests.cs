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
        // Use an in-memory SQLite DB for testing
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection)
            .Options;

        _context = new MealPlannerDBContext(contextOptions);
        _context.Database.EnsureCreated();

        // Add a test user to satisfy FK constraints for Meals
        var testUser = new User
        {
            Id = "user-1",
            FullName = "testuser"
        };
        _context.Users.Add(testUser);
        _context.SaveChanges();

        // Fake logged-in user for ControllerContext
        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Name, "testuser")
            },
            authenticationType: "TestAuth"));

        // Mock services
        var registrationServiceMock = new Mock<IRegistrationService>();
        registrationServiceMock
            .Setup(r => r.FindUserByClaimAsync(_user))
            .ReturnsAsync(testUser);

        var recipeRepoMock = new Mock<IRecipeRepository>();
        var mealRepoMock = new Mock<IMealRepository>();

        // Instantiate controller with mocks and context
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
        // Meal must reference existing user
        var meal = new Meal
        {
            Id = 1,
            UserId = "user-1",
            Recipes = new List<Recipe>() // Empty recipe list is fine
        };
        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<ViewResult>(), "Expected a ViewResult when the meal exists for the user");
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

        Assert.That(result, Is.Not.Null, "ViewResult should not be null");
        Assert.That(result!.Model, Is.TypeOf<Meal>(), "View model should be of type Meal");

        var model = (Meal)result.Model!;
        Assert.That(model.Id, Is.EqualTo(1));
        Assert.That(model.UserId, Is.EqualTo("user-1"));
    }

    [Test]
    public async Task ViewMeal_IncludesRecipes()
    {
        // Add recipes to DB to satisfy FK
        var recipe1 = new Recipe { Id = 1, Name = "Recipe 1", Directions = "D1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe 2", Directions = "D2" };
        _context.Recipes.AddRange(recipe1, recipe2);
        _context.SaveChanges();

        // Meal referencing the recipes
        var meal = new Meal
        {
            Id = 1,
            UserId = "user-1",
            Recipes = new List<Recipe> { recipe1, recipe2 }
        };
        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1) as ViewResult;
        var model = (Meal)result!.Model!;

        Assert.That(model.Recipes, Is.Not.Null, "Meal recipes should not be null");
        Assert.That(model.Recipes.Count, Is.EqualTo(2), "Meal should include both recipes");
        Assert.That(model.Recipes[0].Name, Is.EqualTo("Recipe 1"));
        Assert.That(model.Recipes[1].Name, Is.EqualTo("Recipe 2"));
    }

    [Test]
    public async Task ViewMeal_WhenMealDoesNotExist_ReturnsNotFound()
    {
        var result = await _controller.ViewMeal(999);

        Assert.That(result, Is.TypeOf<NotFoundResult>(), "Nonexistent meal should return NotFoundResult");
    }

    [Test]
    public async Task ViewMeal_WhenMealBelongsToDifferentUser_ReturnsNotFound()
    {
        // Add a different user for FK constraint
        var otherUser = new User { Id = "other-user", FullName = "Other User" };
        _context.Users.Add(otherUser);
        _context.SaveChanges();

        var meal = new Meal
        {
            Id = 1,
            UserId = "other-user",
            Recipes = new List<Recipe>()
        };
        _context.Meals.Add(meal);
        _context.SaveChanges();

        var result = await _controller.ViewMeal(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>(), "Meal belonging to another user should return NotFoundResult");
    }
}