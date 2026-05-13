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

    // --- GetMealSize ---
    // MealSize.Calories() calibrated against a ~2000 cal/day reference:
    //   SmallSnack=200, Small=400, LargeSnack=600, Average=800, Large=1200

    [Test]
    public void GetMealSize_NoTarget_ExactSmallSnackCalories_ReturnsSmallSnack()
    {
        var pref = new UserNutritionPreference();
        Assert.That(pref.GetMealSize(MealWithCalories(200)), Is.EqualTo(MealSize.SmallSnack));
    }

    [Test]
    public void GetMealSize_NoTarget_ExactSmallCalories_ReturnsSmall()
    {
        var pref = new UserNutritionPreference();
        Assert.That(pref.GetMealSize(MealWithCalories(400)), Is.EqualTo(MealSize.Small));
    }

    [Test]
    public void GetMealSize_NoTarget_ExactAverageCalories_ReturnsAverage()
    {
        var pref = new UserNutritionPreference();
        Assert.That(pref.GetMealSize(MealWithCalories(800)), Is.EqualTo(MealSize.Average));
    }

    [Test]
    public void GetMealSize_NoTarget_ExactLargeCalories_ReturnsLarge()
    {
        var pref = new UserNutritionPreference();
        Assert.That(pref.GetMealSize(MealWithCalories(1200)), Is.EqualTo(MealSize.Large));
    }

    [Test]
    public void GetMealSize_NoTarget_ZeroCalories_ReturnsSmallSnack()
    {
        var pref = new UserNutritionPreference();
        Assert.That(pref.GetMealSize(MealWithCalories(0)), Is.EqualTo(MealSize.SmallSnack));
    }

    [Test]
    public void GetMealSize_HighTarget_SameMealCaloriesClassifiesSmaller()
    {
        // 4000 cal/day target = 2× reference. Scaled SmallSnack threshold = 400.
        // A 400-cal meal is an exact match → SmallSnack.
        var pref = new UserNutritionPreference { CalorieTarget = 4000 };
        Assert.That(pref.GetMealSize(MealWithCalories(400)), Is.EqualTo(MealSize.SmallSnack));
    }

    [Test]
    public void GetMealSize_LowTarget_SameMealCaloriesClassifiesLarger()
    {
        // 1000 cal/day target = 0.5× reference. Scaled Average threshold = 400.
        // A 400-cal meal is an exact match → Average.
        var pref = new UserNutritionPreference { CalorieTarget = 1000 };
        Assert.That(pref.GetMealSize(MealWithCalories(400)), Is.EqualTo(MealSize.Average));
    }

    [Test]
    public void GetMealSize_EmptyMeal_ReturnsSmallSnack()
    {
        var pref = new UserNutritionPreference { CalorieTarget = 2000 };
        Assert.That(pref.GetMealSize(new Meal { Recipes = [] }), Is.EqualTo(MealSize.SmallSnack));
    }

    private static Meal MealWithCalories(int calories) => new()
    {
        Recipes = calories == 0 ? [] : [new Recipe { Calories = calories, Tags = [] }]
    };
}
