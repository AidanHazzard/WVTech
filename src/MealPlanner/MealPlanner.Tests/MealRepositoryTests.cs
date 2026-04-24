using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class MealRepositoryTests
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

        context.Users.Add(new User { Id = "user-1", UserName = "user-1", NormalizedUserName = "USER-1", Email = "u@test.com", NormalizedEmail = "U@TEST.COM", SecurityStamp = "stamp" });
        context.SaveChanges();

        var recipe1 = new Recipe { Name = "R1", Directions = "" };
        var recipe2 = new Recipe { Name = "R2", Directions = "" };
        context.AddRange(recipe1, recipe2);
        context.SaveChanges();

        context.AddRange(
            new Meal { Title = "Meal A", UserId = "user-1", StartTime = DateTime.Today, Recipes = [recipe1] },
            new Meal { Title = "Meal B", UserId = "user-1", StartTime = DateTime.Today, Recipes = [recipe2] },
            new Meal { Title = "Meal C", UserId = "user-1", StartTime = DateTime.Today }
        );

        context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _connection.Dispose();

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    int IdOf(MealPlannerDBContext ctx, string title) =>
        ctx.Set<Meal>().First(m => m.Title == title).Id;

    [Test]
    public async Task GetMealsByIdsAsync_ReturnsOnlyRequestedIds()
    {
        using var context = CreateContext();
        var repo = new MealRepository(context);
        var idA = IdOf(context, "Meal A");
        var idC = IdOf(context, "Meal C");

        var result = await repo.GetMealsByIdsAsync([idA, idC]);

        Assert.That(result.Select(m => m.Id), Is.EquivalentTo(new[] { idA, idC }));
    }

    [Test]
    public async Task GetMealsByIdsAsync_ExcludesMealsNotInList()
    {
        using var context = CreateContext();
        var repo = new MealRepository(context);
        var idA = IdOf(context, "Meal A");
        var idB = IdOf(context, "Meal B");
        var idC = IdOf(context, "Meal C");

        var result = await repo.GetMealsByIdsAsync([idA]);

        Assert.That(result.Any(m => m.Id == idB), Is.False);
        Assert.That(result.Any(m => m.Id == idC), Is.False);
    }

    [Test]
    public async Task GetMealsByIdsAsync_IncludesRecipes()
    {
        using var context = CreateContext();
        var repo = new MealRepository(context);
        var idA = IdOf(context, "Meal A");

        var result = await repo.GetMealsByIdsAsync([idA]);

        var meal = result.Single();
        Assert.That(meal.Recipes, Is.Not.Empty);
        Assert.That(meal.Recipes[0].Name, Is.EqualTo("R1"));
    }

    [Test]
    public async Task GetMealsByIdsAsync_WithEmptyList_ReturnsEmpty()
    {
        using var context = CreateContext();
        var repo = new MealRepository(context);

        var result = await repo.GetMealsByIdsAsync([]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetMealsByIdsAsync_WithNonExistentId_IgnoresIt()
    {
        using var context = CreateContext();
        var repo = new MealRepository(context);
        var idA = IdOf(context, "Meal A");

        var result = await repo.GetMealsByIdsAsync([idA, 999999]);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(idA));
    }
}
