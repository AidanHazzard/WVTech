using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MealPlanner.DAL;
using MealPlanner.Models;

namespace MealPlanner.Tests;

public class DietaryRestrictionServiceTests
{
    private static MealPlannerDBContext MakeDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new MealPlannerDBContext(options);
    }

    [Fact]
    public async Task UpdateUserRestrictions_AddsNewRestrictions()
    {
        using var db = MakeDb(Guid.NewGuid().ToString());
        db.Users.Add(new User { Id = 1, FirstName="A", LastName="B", PhoneNumber="1", Email="a@b.com", PasswordHash="x", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow });
        db.DietaryRestrictions.AddRange(
            new DietaryRestriction { Id = 1, Name = "Vegan" },
            new DietaryRestriction { Id = 2, Name = "Gluten-Free" }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionsAsync(1, new[] { 1, 2 });

        var ids = await service.GetUserRestrictionIdsAsync(1);
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public async Task UpdateUserRestrictions_RemovesUnselectedRestrictions()
    {
        using var db = MakeDb(Guid.NewGuid().ToString());
        db.Users.Add(new User { Id = 1, FirstName="A", LastName="B", PhoneNumber="1", Email="a@b.com", PasswordHash="x", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow });
        db.DietaryRestrictions.AddRange(
            new DietaryRestriction { Id = 1, Name = "Vegan" },
            new DietaryRestriction { Id = 2, Name = "Gluten-Free" }
        );
        db.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = 1, DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = 1, DietaryRestrictionId = 2 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionsAsync(1, new[] { 2 });

        var ids = await service.GetUserRestrictionIdsAsync(1);
        Assert.DoesNotContain(1, ids);
        Assert.Contains(2, ids);
        Assert.Single(ids);
    }

    [Fact]
    public async Task UpdateUserRestrictions_DoesNotDuplicateExistingSelections()
    {
        using var db = MakeDb(Guid.NewGuid().ToString());
        db.Users.Add(new User { Id = 1, FirstName="A", LastName="B", PhoneNumber="1", Email="a@b.com", PasswordHash="x", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow });
        db.DietaryRestrictions.Add(new DietaryRestriction { Id = 1, Name = "Vegan" });
        db.UserDietaryRestrictions.Add(new UserDietaryRestriction { UserId = 1, DietaryRestrictionId = 1 });
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionsAsync(1, new[] { 1, 1, 1 });

        var count = await db.UserDietaryRestrictions.CountAsync();
        Assert.Equal(1, count);
    }
}
