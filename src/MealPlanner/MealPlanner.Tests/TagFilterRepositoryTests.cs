using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Tests;

[TestFixture]
public class TagFilterRepositoryTests
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
    }

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    [TearDown]
    public void TearDown() => _connection.Dispose();

    [Test]
    public void GetRecipesByNameAndTag_ReturnsRecipesWithMatchingTag()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Breakfast" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Porridge", Directions = "", Tags = [tag] });
        context.Recipes.Add(new Recipe { Name = "Pasta Bolognese", Directions = "" });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("", "Breakfast");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Name, Is.EqualTo("Oatmeal Porridge"));
    }

    [Test]
    public void GetRecipesByNameAndTag_ReturnsEmptyList_WhenNoRecipesHaveTag()
    {
        // Arrange
        using var context = CreateContext();
        context.Recipes.Add(new Recipe { Name = "Pasta Bolognese", Directions = "" });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("", "Breakfast");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetRecipesByNameAndTag_FiltersOnBothNameAndTag_WhenNameProvided()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Breakfast" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Bowl", Directions = "", Tags = [tag] });
        context.Recipes.Add(new Recipe { Name = "Scrambled Eggs", Directions = "", Tags = [tag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("Oatmeal", "Breakfast");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Name, Is.EqualTo("Oatmeal Bowl"));
    }

    [Test]
    public void GetRecipesByNameAndTag_ReturnsEmpty_WhenNameMatchesButTagDoesNot()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Dinner" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Bowl", Directions = "", Tags = [tag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("Oatmeal", "Breakfast");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetRecipesByNameAndTag_IsCaseInsensitive_ForTagName()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Breakfast" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Porridge", Directions = "", Tags = [tag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act — uppercase tag name
        var results = repo.GetRecipesByNameAndTag("", "BREAKFAST");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetRecipesByNameAndTag_ExcludesExternalRecipes()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Breakfast" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe
        {
            Name = "Oatmeal (External)",
            Directions = "",
            ExternalUri = "https://example.com/recipe",
            Tags = [tag]
        });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("", "Breakfast");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetRecipesByNameAndTag_ReturnsMultipleRecipes_WithSameTag()
    {
        // Arrange
        using var context = CreateContext();
        var tag = new Tag { Name = "Breakfast" };
        context.Tags.Add(tag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Porridge", Directions = "", Tags = [tag] });
        context.Recipes.Add(new Recipe { Name = "Scrambled Eggs", Directions = "", Tags = [tag] });
        context.Recipes.Add(new Recipe { Name = "Pasta Bolognese", Directions = "" });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("", "Breakfast");

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetRecipesByNameAndTag_ReturnsAll_WhenNameIsEmptyAndTagMatches()
    {
        // Arrange
        using var context = CreateContext();
        var breakfastTag = new Tag { Name = "Breakfast" };
        var dinnerTag = new Tag { Name = "Dinner" };
        context.Tags.AddRange(breakfastTag, dinnerTag);
        context.Recipes.Add(new Recipe { Name = "Oatmeal Porridge", Directions = "", Tags = [breakfastTag] });
        context.Recipes.Add(new Recipe { Name = "Scrambled Eggs", Directions = "", Tags = [breakfastTag] });
        context.Recipes.Add(new Recipe { Name = "Grilled Salmon", Directions = "", Tags = [dinnerTag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        // Act
        var results = repo.GetRecipesByNameAndTag("", "Breakfast");

        // Assert — only breakfast recipes returned, not dinner or untagged
        Assert.That(results.Select(r => r.Name), Is.EquivalentTo(new[] { "Oatmeal Porridge", "Scrambled Eggs" }));
    }
}
