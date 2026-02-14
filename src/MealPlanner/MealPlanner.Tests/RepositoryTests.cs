using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Tests;

[TestFixture]
public class RepositoryTests
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
                new Recipe { Name="R1", Directions="", Ingredients="" },
                new Recipe { Name="R2", Directions="", Ingredients="" },
                new Recipe { Name="R3", Directions="", Ingredients="" }
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
    public void Read_ReturnsEntityById()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        Recipe toFind = context.Set<Recipe>().First();

        // Act
        Recipe? found = repo.Read(toFind.Id);

        // Assert
        Assert.That(found?.Name, Is.EqualTo(toFind.Name), "Found Name should match toFind Name");
    }
    
    [Test]
    public void Read_ReturnsNullWhenIdNotFound()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        
        // Act
        Recipe? found = repo.Read(300);

        // Assert
        Assert.That(found, Is.Null, "Read should return null if id is not in the db");
    }

    [Test]
    public void ReadAll_ReturnsMultipleEntities()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        int expectedLength = context.Set<Recipe>().ToList().Count;

        // Act
        int foundLength = repo.ReadAll().Count();

        // Assert
        Assert.That(foundLength, Is.EqualTo(expectedLength), "Number of elements in repository should be same as number of elements in table");
    }

    [Test]
    public void CreateOrUpdate_MakesANewRowInTableIfEntityIsNotInTable()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        int lengthBefore = context.Set<Recipe>().ToList().Count;

        // Act
        repo.CreateOrUpdate(new Recipe { Name="Test", Directions="", Ingredients="" });
        context.SaveChanges();
        int lengthAfter = context.Set<Recipe>().ToList().Count;

        // Assert
        Assert.That(lengthAfter, Is.EqualTo(lengthBefore + 1), "Length after should be 1 greater than length before");
    }

    [Test]
    public void CreateOrUpdate_ChangesExistingRowOnTable()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        Recipe recipe = context.Set<Recipe>().First();
        string oldName = recipe.Name;
        int recipeId = recipe.Id;

        // Act
        recipe.Name = "New Name";
        repo.CreateOrUpdate(recipe);
        context.SaveChanges();

        // Assert
        Assert.That(context.Find<Recipe>(recipeId)?.Name, Is.Not.Null.And.Not.EqualTo(oldName), "Recipe.Name in context should be changed");
    }

    [Test]
    public void Delete_RemovesEntityFromTable()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        int beforeLength = context.Set<Recipe>().Count();

        // Act
        repo.Delete(context.Set<Recipe>().First());
        context.SaveChanges();
        int afterLength = context.Set<Recipe>().Count();

        // Assert
        Assert.That(afterLength, Is.EqualTo(beforeLength - 1), "After length should be less than before length");
    }

    [Test]
    public void Exists_ReturnsTrueIfIdFoundInTable()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);
        int toFindId = context.Set<Recipe>().First().Id;

        // Act
        bool exists = repo.Exists(toFindId);

        // Assert
        Assert.That(exists, Is.True, "Id in table should return true");
    }

    [Test]
    public void Exists_ReturnsFalseIfIdNotFoundInTable()
    {
        // Arrange
        MealPlannerDBContext context = CreateContext();
        Repository<Recipe> repo = new Repository<Recipe>(context);

        // Act
        bool doesntExist = repo.Exists(300);

        // Assert
        Assert.That(doesntExist, Is.False, "Id not in table should return false");
    }
}
