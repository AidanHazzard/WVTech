using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

using MealPlanner.Models;

public class UserNutritionPreferenceTests
{
    private static MealPlannerDBContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MealPlannerDBContext(options);
    }

    private static async Task SeedUsersAsync(MealPlannerDBContext db)
    {
        db.Users.AddRange(
            new User
            {
                Id = "userA",
                UserName = "userA",
                NormalizedUserName = "USERA",
                FullName = "Test User A"
            },
            new User
            {
                Id = "userB",
                UserName = "userB",
                NormalizedUserName = "USERB",
                FullName = "Test User B"
            }
        );

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CanCreateNutritionPreference_ForUser()
    {
        await using var db = CreateDb();
        await SeedUsersAsync(db);

        var pref = new UserNutritionPreference
        {
            UserId = "userA",
            CalorieTarget = 2500,
            ProteinTarget = 180,
            CarbTarget = 300,
            FatTarget = 70
        };

        db.UserNutritionPreferences.Add(pref);
        await db.SaveChangesAsync();

        var saved = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");

        Assert.True(saved.Id > 0);
        Assert.Equal(2500, saved.CalorieTarget);
        Assert.Equal(180, saved.ProteinTarget);
        Assert.Equal(300, saved.CarbTarget);
        Assert.Equal(70, saved.FatTarget);
    }

    [Fact]
    public async Task CanUpdateNutritionPreference_ForUser()
    {
        await using var db = CreateDb();
        await SeedUsersAsync(db);

        db.UserNutritionPreferences.Add(new UserNutritionPreference
        {
            UserId = "userA",
            CalorieTarget = 2000,
            ProteinTarget = 150
        });
        await db.SaveChangesAsync();

        var existing = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");

        existing.CalorieTarget = 2400;
        existing.ProteinTarget = 175;
        existing.IronTarget = 18;
        existing.FiberTarget = 30;

        await db.SaveChangesAsync();

        var updated = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        Assert.Equal(2400, updated.CalorieTarget);
        Assert.Equal(175, updated.ProteinTarget);
        Assert.Equal(18, updated.IronTarget);
        Assert.Equal(30, updated.FiberTarget);
    }

    [Fact]
    public async Task Preferences_AreIsolatedBetweenUsers()
    {
        await using var db = CreateDb();
        await SeedUsersAsync(db);

        db.UserNutritionPreferences.AddRange(
            new UserNutritionPreference
            {
                UserId = "userA",
                CalorieTarget = 2300,
                ProteinTarget = 160
            },
            new UserNutritionPreference
            {
                UserId = "userB",
                CalorieTarget = 1800,
                ProteinTarget = 120
            }
        );

        await db.SaveChangesAsync();

        var a = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        var b = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userB");

        Assert.Equal(2300, a.CalorieTarget);
        Assert.Equal(160, a.ProteinTarget);

        Assert.Equal(1800, b.CalorieTarget);
        Assert.Equal(120, b.ProteinTarget);
    }

    [Fact]
    public async Task CanQueryPreferenceByUserId()
    {
        await using var db = CreateDb();
        await SeedUsersAsync(db);

        db.UserNutritionPreferences.Add(new UserNutritionPreference
        {
            UserId = "userA",
            VitaminATarget = 900,
            VitaminCTarget = 90,
            B12Target = 2
        });
        await db.SaveChangesAsync();

        var pref = await db.UserNutritionPreferences
            .Where(x => x.UserId == "userA")
            .Select(x => new
            {
                x.VitaminATarget,
                x.VitaminCTarget,
                x.B12Target
            })
            .SingleAsync();

        Assert.Equal(900, pref.VitaminATarget);
        Assert.Equal(90, pref.VitaminCTarget);
        Assert.Equal(2, pref.B12Target);
    }

    [Fact]
    public async Task CanStoreNullTargets_AndLaterSetThem()
    {
        await using var db = CreateDb();
        await SeedUsersAsync(db);

        db.UserNutritionPreferences.Add(new UserNutritionPreference
        {
            UserId = "userA",
            CalorieTarget = null,
            ProteinTarget = null
        });
        await db.SaveChangesAsync();

        var pref = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        Assert.Null(pref.CalorieTarget);
        Assert.Null(pref.ProteinTarget);

        pref.CalorieTarget = 2100;
        pref.ProteinTarget = 155;

        await db.SaveChangesAsync();

        var updated = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        Assert.Equal(2100, updated.CalorieTarget);
        Assert.Equal(155, updated.ProteinTarget);
    }
}
