using System;
using System.Linq;
using System.Threading.Tasks;
using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class FavoritesServiceTests
{
    private static MealPlannerDBContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MealPlannerDBContext(options);
    }

    [Test]
    public async Task AddFavoriteAsync_Adds_WhenNotAlreadyFavorited()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        await service.AddFavoriteAsync("user-1", 10);

        var row = await db.UserFavoriteRecipes.SingleAsync();
        Assert.That(row.UserId, Is.EqualTo("user-1"));
        Assert.That(row.RecipeId, Is.EqualTo(10));
    }

    [Test]
    public async Task AddFavoriteAsync_DoesNotDuplicate_WhenAlreadyFavorited()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        await service.AddFavoriteAsync("user-1", 10);
        await service.AddFavoriteAsync("user-1", 10);

        var count = await db.UserFavoriteRecipes.CountAsync();
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveFavoriteAsync_Removes_WhenExists()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        await service.AddFavoriteAsync("user-1", 10);
        await service.RemoveFavoriteAsync("user-1", 10);

        var count = await db.UserFavoriteRecipes.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveFavoriteAsync_DoesNothing_WhenMissing()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        await service.RemoveFavoriteAsync("user-1", 999);

        var count = await db.UserFavoriteRecipes.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task IsFavoritedAsync_ReturnsFalse_WhenMissing()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        var result = await service.IsFavoritedAsync("user-1", 10);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFavoritedAsync_ReturnsTrue_WhenExists()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        await service.AddFavoriteAsync("user-1", 10);

        var result = await service.IsFavoritedAsync("user-1", 10);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsRecipes_ForUser()
    {
        await using var db = CreateDb();

        var r1 = new Recipe { Id = 1, Name = "R1", Directions = "D1" };
        var r2 = new Recipe { Id = 2, Name = "R2", Directions = "D2" };
        db.Recipes.AddRange(r1, r2);

        db.UserFavoriteRecipes.AddRange(
            new UserFavoriteRecipe { UserId = "user-1", RecipeId = 1, Recipe = r1 },
            new UserFavoriteRecipe { UserId = "user-1", RecipeId = 2, Recipe = r2 },
            new UserFavoriteRecipe { UserId = "user-2", RecipeId = 2, Recipe = r2 }
        );

        await db.SaveChangesAsync();

        var service = new FavoritesService(db);

        var favorites = await service.GetFavoritesAsync("user-1");

        Assert.That(favorites, Has.Count.EqualTo(2));
        Assert.That(favorites.Any(r => r.Id == 1), Is.True);
        Assert.That(favorites.Any(r => r.Id == 2), Is.True);
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsEmpty_WhenUserHasNoFavorites()
    {
        await using var db = CreateDb();
        var service = new FavoritesService(db);

        var favorites = await service.GetFavoritesAsync("user-1");

        Assert.That(favorites, Is.Empty);
    }
}