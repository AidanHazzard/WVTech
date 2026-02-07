using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Tests;

[TestFixture]
public class RecipeRepositoryTests
{
    private DbConnection _connection;
    private DbContextOptions<MealPlannerDBContext> _contextOptions;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(_connection)
            .Options;
        
        using var context = new MealPlannerDBContext(_contextOptions);

        if (context.Database.EnsureCreated())
        {
            context.AddRange(
                new Recipe { Name="Oatmeal Cookies", Directions="", Ingredients="" },
                new Recipe { Name="Spaghetti All'assassina", Directions="", Ingredients="" },
                new Recipe { Name="Spaghetti and Meatballs", Directions="", Ingredients="" },
                new Recipe { Name="Vegan Spaghetti with Mushrooms", Directions="", Ingredients="" },
                new Recipe { Name="Baked Spaghetti Casserole", Directions="", Ingredients="" },
                new Recipe { Name="Mac 'n Cheese Casserole", Directions="", Ingredients="" },
                new Recipe { Name="Homemade Mac 'n Cheese", Directions="", Ingredients="" },
                new Recipe { Name="Mushroom Steak Salad", Directions="", Ingredients="" },
                new Recipe { Name="Ceasar Salad", Directions="", Ingredients="" }
            );

            context.SaveChanges();
        }
    }

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
    }
    
    [Test]
    public void GetRecipesByNameReturnsARecipe()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("Oatmeal Cookies");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Oatmeal Cookies"));
    }
    
    [Test]
    public void GetRecipesByNameReturnsEmptyListIfNoneFound()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("I don't exist");

        // Assert
        Assert.That(results, Is.Empty);
    }
    
    [Test]
    public void GetRecipesByNameMatchesBeginningOfName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("Home");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Homemade Mac 'n Cheese"));
    }
    
    [Test]
    public void GetRecipesByNameIsNotCaseSensitive()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("baked spaghetti casserole");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Baked Spaghetti Casserole"));
    }
    
    [Test]
    public void GetRecipesByNameMatchesWordsInMiddleOfRecipeName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("Spaghetti");

        // Assert
        Assert.That(results.Count(), Is.EqualTo(4), "There are 4 recipes with spaghetti in their name");
    }
    
    [Test]
    public void GetRecipesByNameMatchesBeginningOfWordsInMiddleOfRecipeName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("Cass");

        // Assert
        Assert.That(results.Count(), Is.EqualTo(2), "There are 2 recipes with words that start with Cass");
    }
    
    [Test]
    public void GetRecipesByNameDoesntMatchMiddleOfWords()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        IQueryable<Recipe> results = repo.GetRecipesByName("aghe");

        // Assert
        Assert.That(results, Is.Empty);
    }
}