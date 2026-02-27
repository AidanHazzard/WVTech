using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class RecipeAPIControllerTests
{
    [Test]
    public void SearchRecipesByName_ReturnsStatus200OK()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([new Recipe{Id=1, Name = "Oatmeal Cookies", Directions = ""}]);
        var controller = new RecipeAPIController(repo.Object);

        // Act
        var result = controller.SearchRecipesByName(searchTerm);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public void SearchRecipesByName_ReturnsStatus404NotFound()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([]);
        var controller = new RecipeAPIController(repo.Object);
        
        // Act
        var result = controller.SearchRecipesByName(searchTerm);

        // Assert

        // If we want to add an error message later, the class that would return inherits from object result
        // rather than status code result, so testing for both for potential future proofing
        Assert.That(result, Is.TypeOf<NotFoundResult>().Or.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public void SearchRecipesByName_ReturnsOneRecipe()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([new Recipe{Id=1, Name = "Oatmeal Cookies", Directions = ""}]);
        var controller = new RecipeAPIController(repo.Object);
        
        // Act
        var result = controller.SearchRecipesByName(searchTerm) as OkObjectResult;
        var recipe = result?.Value as List<Recipe>;

        // Assert
        Assert.That(recipe?[0].Name, Is.EqualTo("Oatmeal Cookies"));
    }

    [Test]
    public void SearchRecipesByNameReturns_MultipleRecipes()
    {
        // Arrange
        string searchTerm = "spagh";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns(
            [
                new Recipe { Name="Spaghetti All'assassina", Directions=""},
                new Recipe { Name="Spaghetti and Meatballs", Directions=""},
                new Recipe { Name="Vegan Spaghetti with Mushrooms", Directions=""},
                new Recipe { Name="Baked Spaghetti Casserole", Directions=""}
            ]
        );
        var controller = new RecipeAPIController(repo.Object);

        // Act
        var result = controller.SearchRecipesByName(searchTerm) as OkObjectResult;
        var recipes = result?.Value as List<Recipe>;

        // Assert
        Assert.That(recipes?.Count, Is.EqualTo(4));
    }
}