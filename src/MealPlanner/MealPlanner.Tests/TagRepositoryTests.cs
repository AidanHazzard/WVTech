using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Tests;

[TestFixture]
public class TagRepositoryTests
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

        context.Tags.AddRange(
            new Tag { Name = "Breakfast" },
            new Tag { Name = "Dinner" },
            new Tag { Name = "Lunch" }
        );
        context.SaveChanges();
    }

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    [TearDown]
    public void TearDown() => _connection.Dispose();

    [Test]
    public async Task GetTagNamesAsync_ReturnsAllTagNames()
    {
        // Arrange
        var repo = new TagRepository(CreateContext());

        // Act
        var names = await repo.GetTagNamesAsync();

        // Assert
        Assert.That(names, Contains.Item("Breakfast"));
        Assert.That(names, Contains.Item("Dinner"));
        Assert.That(names, Contains.Item("Lunch"));
    }

    [Test]
    public async Task GetTagNamesAsync_ReturnsNamesInAlphabeticalOrder()
    {
        // Arrange
        var repo = new TagRepository(CreateContext());

        // Act
        var names = await repo.GetTagNamesAsync();

        // Assert
        Assert.That(names, Is.EqualTo(names.OrderBy(n => n).ToList()));
    }

    [Test]
    public async Task GetTagNamesAsync_ReturnsEmptyList_WhenNoTagsExist()
    {
        // Arrange
        using var context = CreateContext();
        context.Tags.RemoveRange(context.Tags.ToList());
        context.SaveChanges();
        var repo = new TagRepository(context);

        // Act
        var names = await repo.GetTagNamesAsync();

        // Assert
        Assert.That(names, Is.Empty);
    }
}
