using System;
using System.Linq;
using System.Threading.Tasks;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
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

    [Test]
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

        Assert.That(saved.Id, Is.GreaterThan(0));
        Assert.That(saved.CalorieTarget, Is.EqualTo(2500));
        Assert.That(saved.ProteinTarget, Is.EqualTo(180));
        Assert.That(saved.CarbTarget, Is.EqualTo(300));
        Assert.That(saved.FatTarget, Is.EqualTo(70));
    }

    [Test]
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
        existing.CarbTarget = 250;
        existing.FatTarget = 80;

        await db.SaveChangesAsync();

        var updated = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        Assert.That(updated.CalorieTarget, Is.EqualTo(2400));
        Assert.That(updated.ProteinTarget, Is.EqualTo(175));
        Assert.That(updated.CarbTarget, Is.EqualTo(250));
        Assert.That(updated.FatTarget, Is.EqualTo(80));
    }

    [Test]
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

        Assert.That(a.CalorieTarget, Is.EqualTo(2300));
        Assert.That(a.ProteinTarget, Is.EqualTo(160));

        Assert.That(b.CalorieTarget, Is.EqualTo(1800));
        Assert.That(b.ProteinTarget, Is.EqualTo(120));
    }

    [Test]
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
        Assert.That(pref.CalorieTarget, Is.Null);
        Assert.That(pref.ProteinTarget, Is.Null);

        pref.CalorieTarget = 2100;
        pref.ProteinTarget = 155;

        await db.SaveChangesAsync();

        var updated = await db.UserNutritionPreferences.SingleAsync(x => x.UserId == "userA");
        Assert.That(updated.CalorieTarget, Is.EqualTo(2100));
        Assert.That(updated.ProteinTarget, Is.EqualTo(155));
    }
}
