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
            // Normally we'd add in test data here, but we will use the seed data definined in MealPlannerDBContext
            // This is just to build the model in the first place
            context.AddRange(
                new Recipe { Name="R1", Directions="" }
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
    public void GetRecipesByName_ReturnsARecipe()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("Oatmeal Cookies");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Oatmeal Cookies"));
    }
    
    [Test]
    public void GetRecipesByName_ReturnsEmptyListIfNoneFound()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("I don't exist");

        // Assert
        Assert.That(results, Is.Empty);
    }
    
    [Test]
    public void GetRecipesByName_MatchesBeginningOfName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("Home");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Homemade Mac 'n Cheese"));
    }
    
    [Test]
    public void GetRecipesByName_IsNotCaseSensitive()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("baked spaghetti casserole");

        // Assert
        Assert.That(results.First().Name, Is.EqualTo("Baked Spaghetti Casserole"));
    }
    
    [Test]
    public void GetRecipesByName_MatchesWordsInMiddleOfRecipeName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("Spaghetti");

        // Assert
        foreach (Recipe r in results)
        {
            Console.WriteLine(r.Name);
        }
        
        Assert.That(results.Count(), Is.EqualTo(4), "There are 4 recipes with spaghetti in their name");
    }
    
    [Test]
    public void GetRecipesByName_MatchesBeginningOfWordsInMiddleOfRecipeName()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("Cass");

        // Assert
        Console.Write(results);
        Assert.That(results.Count(), Is.EqualTo(2), "There are 2 recipes with words that start with Cass");
    }
    
    [Test]
    public void GetRecipesByName_DoesntMatchMiddleOfWords()
    {   
        // Arrange
        MealPlannerDBContext context = CreateContext();
        RecipeRepository repo = new RecipeRepository(context);

        // Act
        List<Recipe> results = repo.GetRecipesByName("aghe");

        // Assert
        Assert.That(results, Is.Empty);
    }
}