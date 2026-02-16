using System;
using System.Linq;
using System.Threading.Tasks;
using MealPlanner.DAL;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class DietaryRestrictionServiceTests
{
    private static MealPlannerDBContext MakeDb()
    {
        var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MealPlannerDBContext(options);
    }

    private static async Task SeedBaseAsync(MealPlannerDBContext db)
    {
        db.Users.Add(new User
        {
            Id = "userA",
            UserName = "userA",
            NormalizedUserName = "USERA",
            FullName = "Test User A"
        });

        db.DietaryRestrictions.AddRange(
            new DietaryRestriction { Id = 1, Name = "Vegan" },
            new DietaryRestriction { Id = 2, Name = "Gluten-Free" }
        );

        await db.SaveChangesAsync();
    }

    [Test]
    public async Task UpdateUserRestriction_AddsNewRestrictions()
    {
        await using var db = MakeDb();
        await SeedBaseAsync(db);

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionAsync("userA", new[] { 1, 2 });

        var ids = await service.GetUserRestrictionIdsAsync("userA");

        Assert.That(ids.Count, Is.EqualTo(2));
        Assert.That(ids, Does.Contain(1));
        Assert.That(ids, Does.Contain(2));
    }

    [Test]
    public async Task UpdateUserRestriction_RemovesUnselectedRestrictions()
    {
        await using var db = MakeDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 2 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionAsync("userA", new[] { 2 });

        var ids = await service.GetUserRestrictionIdsAsync("userA");

        Assert.That(ids.Count, Is.EqualTo(1));
        Assert.That(ids, Does.Contain(2));
        Assert.That(ids, Does.Not.Contain(1));
    }

    [Test]
    public async Task UpdateUserRestriction_DoesNotDuplicateExistingSelections()
    {
        await using var db = MakeDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.Add(new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 });
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);
        await service.UpdateUserRestrictionAsync("userA", new[] { 1, 1, 1 });

        var count = await db.UserDietaryRestrictions.CountAsync(x => x.UserId == "userA");
        Assert.That(count, Is.EqualTo(1));
    }
}
