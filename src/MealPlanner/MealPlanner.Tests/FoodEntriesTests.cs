using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class FoodEntriesTests
{
    private FoodEntriesController _controller;

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection)
            .Options;
        
        using var context = new MealPlannerDBContext(contextOptions);

        var _recipeRepo = new Mock<IRecipeRepository>();
        _recipeRepo.Setup(_recipeRepo => _recipeRepo.Read(1)).Returns(new Recipe{Id=1, Name = "Oatmeal Cookies", Directions = "", Ingredients = ""});
        var nutritionService = new Mock<INutritionProgressService>();

        _controller = new FoodEntriesController(_recipeRepo.Object, context, nutritionService.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public void RecipesPlural_ReturnsView()
    {
        // Act
        var result = _controller.Recipes();
        // Assert
        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public void RecipesSingular_ReturnsRecipeView_IfIdIsInRecipeRepo()
    {
        // Act
        var result = _controller.Recipes(1) as ViewResult;
        // Assert
        Assert.That(result?.ViewName, Is.EqualTo("SingleRecipe"));
    }

    [Test]
    public void RecipesSingular_Redirects_IfIdNotInRecipeRepo()
    {
        // Act
        var result = _controller.Recipes(-1) as RedirectToActionResult;
        // Assert
        Assert.That(result?.ActionName, Is.EqualTo("SelectType"));
    }
}