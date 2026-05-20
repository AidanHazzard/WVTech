using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class UserSettingsRepositoryTests
{
    private DbConnection _connection = null!;
    private DbContextOptions<MealPlannerDBContext> _contextOptions;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(_connection)
            .Options;
        using var ctx = new MealPlannerDBContext(_contextOptions);
        ctx.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown() => _connection.Dispose();

    MealPlannerDBContext CreateContext() => new(_contextOptions);

    private void SeedUser(MealPlannerDBContext ctx, string id = "user-1")
    {
        ctx.Users.Add(new User
        {
            Id = id,
            UserName = $"{id}@test.com",
            NormalizedUserName = $"{id}@TEST.COM",
            Email = $"{id}@test.com",
            NormalizedEmail = $"{id}@TEST.COM",
            SecurityStamp = "s"
        });
        ctx.SaveChanges();
    }

    // ── FindOrCreateAsync ────────────────────────────────────────────────────

    [Test]
    public async Task FindOrCreateAsync_ReturnsExistingProfileWhenOneExists()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        ctx.UserProfiles.Add(new UserProfile { UserId = "user-1", DisplayHandle = "gary" });
        ctx.SaveChanges();

        var repoCtx = CreateContext();
        var repo = new UserSettingsRepository(repoCtx);
        var profile = await repo.FindOrCreateAsync("user-1");

        Assert.That(profile.DisplayHandle, Is.EqualTo("gary"));
    }

    [Test]
    public async Task FindOrCreateAsync_CreatesProfileWhenNoneExists()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);

        var repoCtx = CreateContext();
        var repo = new UserSettingsRepository(repoCtx);
        var profile = await repo.FindOrCreateAsync("user-1");
        repoCtx.SaveChanges();

        Assert.That(profile.UserId, Is.EqualTo("user-1"));
        Assert.That(await CreateContext().UserProfiles.CountAsync(p => p.UserId == "user-1"), Is.EqualTo(1));
    }

}
