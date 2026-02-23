using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

using MealPlanner.Models;
using MealPlanner.DAL;

public class DietaryRestrictionServiceTests
{
    private static MealPlannerDBContext CreateDb()
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
            FullName = "Test User"
        });


        db.DietaryRestrictions.AddRange(
            new DietaryRestriction { Id = 1, Name = "Vegan" },
            new DietaryRestriction { Id = 2, Name = "Gluten-Free" },
            new DietaryRestriction { Id = 3, Name = "Keto" },
            new DietaryRestriction { Id = 4, Name = "Halal" }
        );

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllRestrictions_ReturnsAll()
    {
        await using var db = CreateDb();
        await SeedBaseAsync(db);

        var service = new DietaryRestrictionService(db);

        var all = await service.GetAllRestrictionsAsync();

        Assert.Equal(4, all.Count);
        Assert.Contains(all, x => x.Id == 1);
        Assert.Contains(all, x => x.Id == 2);
        Assert.Contains(all, x => x.Id == 3);
        Assert.Contains(all, x => x.Id == 4);
    }

    [Fact]
    public async Task GetUserRestrictionIds_ReturnsOnlyUserSelections()
    {
        await using var db = CreateDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 3 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);

        var selected = await service.GetUserRestrictionIdsAsync("userA");

        Assert.Equal(new[] { 1, 3 }, selected.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task UpdateUserRestriction_AddsAndRemovesCorrectly()
    {
        await using var db = CreateDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 3 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);

        await service.UpdateUserRestrictionAsync("userA", new[] { 2, 3, 4 });

        var now = await db.UserDietaryRestrictions
            .Where(x => x.UserId == "userA")
            .Select(x => x.DietaryRestrictionId)
            .ToListAsync();

        Assert.Equal(new[] { 2, 3, 4 }, now.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task UpdateUserRestriction_EmptySelection_RemovesAll()
    {
        await using var db = CreateDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 2 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);

        await service.UpdateUserRestrictionAsync("userA", Array.Empty<int>());

        var count = await db.UserDietaryRestrictions.CountAsync(x => x.UserId == "userA");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UpdateUserRestriction_DoesNotCreateDuplicates()
    {
        await using var db = CreateDb();
        await SeedBaseAsync(db);

        db.UserDietaryRestrictions.Add(
            new UserDietaryRestriction { UserId = "userA", DietaryRestrictionId = 1 }
        );
        await db.SaveChangesAsync();

        var service = new DietaryRestrictionService(db);

        await service.UpdateUserRestrictionAsync("userA", new[] { 1, 2 });

        var links = await db.UserDietaryRestrictions
            .Where(x => x.UserId == "userA")
            .ToListAsync();

        Assert.Equal(2, links.Count);
        Assert.Equal(2, links.Select(x => x.DietaryRestrictionId).Distinct().Count());
    }
}
