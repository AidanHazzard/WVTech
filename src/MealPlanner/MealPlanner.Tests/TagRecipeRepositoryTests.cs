using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Tests;

[TestFixture]
public class TagRecipeRepositoryTests
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
        context.Database.EnsureCreated();

        context.Tags.Add(new Tag { Name = "Breakfast" });
        context.SaveChanges();
    }

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    [TearDown]
    public void TearDown() => _connection.Dispose();

    [Test]
    public void CreateOrUpdate_AssignsPredefinedTagToRecipe()
    {
        // Arrange
        var context = CreateContext();
        var repo = new RecipeRepository(context);
        var recipe = new Recipe
        {
            Name = "Tagged Recipe",
            Directions = "",
            Tags = [new Tag { Name = "Breakfast" }]
        };

        // Act
        repo.CreateOrUpdate(recipe);
        context.SaveChanges();

        // Assert — tag count should not increase (existing tag was reused)
        Assert.That(context.Tags.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateOrUpdate_CreatesNewTag_WhenTagDoesNotExist()
    {
        // Arrange
        var context = CreateContext();
        var repo = new RecipeRepository(context);
        var recipe = new Recipe
        {
            Name = "Tagged Recipe",
            Directions = "",
            Tags = [new Tag { Name = "Spicy" }]
        };

        // Act
        int tagCountBefore = context.Tags.Count();
        repo.CreateOrUpdate(recipe);
        context.SaveChanges();

        // Assert
        Assert.That(context.Tags.Count(), Is.EqualTo(tagCountBefore + 1));
    }

    [Test]
    public void CreateOrUpdate_DoesNotDuplicate_WhenSameCustomTagUsedTwice()
    {
        // Arrange
        var context = CreateContext();
        var repo = new RecipeRepository(context);
        var recipe = new Recipe
        {
            Name = "Tagged Recipe",
            Directions = "",
            Tags = [new Tag { Name = "Spicy" }, new Tag { Name = "Spicy" }]
        };

        // Act
        int tagCountBefore = context.Tags.Count();
        repo.CreateOrUpdate(recipe);
        context.SaveChanges();

        // Assert — only one new tag row created despite two entries with same name
        Assert.That(context.Tags.Count(), Is.EqualTo(tagCountBefore + 1));
    }

    [Test]
    public async Task ReadRecipeWithIngredientsAsync_IncludesTags()
    {
        // Arrange
        var setupContext = CreateContext();
        var existingTag = setupContext.Tags.First();
        var recipe = new Recipe
        {
            Name = "Tagged Recipe",
            Directions = "",
            Tags = [existingTag]
        };
        setupContext.Add(recipe);
        setupContext.SaveChanges();
        int recipeId = recipe.Id;

        var context = CreateContext();
        var repo = new RecipeRepository(context);

        // Act
        var result = await repo.ReadRecipeWithIngredientsAsync(recipeId);

        // Assert
        Assert.That(result?.Tags, Is.Not.Null);
        Assert.That(result!.Tags.Any(t => t.Name == "Breakfast"), Is.True);
    }
}
