using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class UserDietaryRestrictionRepositoryTests
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

    private void SeedRestrictions(MealPlannerDBContext ctx)
    {
        ctx.DietaryRestrictions.AddRange(
            new DietaryRestriction { Id = 1, Name = "Vegan" },
            new DietaryRestriction { Id = 2, Name = "Gluten-Free" },
            new DietaryRestriction { Id = 3, Name = "Nut-Free" }
        );
        ctx.SaveChanges();
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_ReturnsAllRestrictions()
    {
        using var ctx = CreateContext();
        SeedRestrictions(ctx);

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var result = await repo.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetAllAsync_ReturnsRestrictionsOrderedByName()
    {
        using var ctx = CreateContext();
        SeedRestrictions(ctx);

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var names = (await repo.GetAllAsync()).Select(r => r.Name).ToList();

        Assert.That(names, Is.EqualTo(names.OrderBy(n => n).ToList()));
    }

    [Test]
    public async Task GetAllAsync_ReturnsEmptyListWhenNoRestrictionsDefined()
    {
        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var result = await repo.GetAllAsync();

        Assert.That(result, Is.Empty);
    }

    // ── GetSelectedIdsByUserIdAsync ──────────────────────────────────────────

    [Test]
    public async Task GetSelectedIdsByUserIdAsync_ReturnsIdsForUser()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        SeedRestrictions(ctx);
        ctx.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 3 });
        ctx.SaveChanges();

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var ids = await repo.GetSelectedIdsByUserIdAsync("user-1");

        Assert.That(ids, Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public async Task GetSelectedIdsByUserIdAsync_ReturnsEmptyListWhenUserHasNoRestrictions()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        SeedRestrictions(ctx);

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var ids = await repo.GetSelectedIdsByUserIdAsync("user-1");

        Assert.That(ids, Is.Empty);
    }

    [Test]
    public async Task GetSelectedIdsByUserIdAsync_OnlyReturnsIdsForRequestedUser()
    {
        using var ctx = CreateContext();
        SeedUser(ctx, "user-1");
        SeedUser(ctx, "user-2");
        SeedRestrictions(ctx);
        ctx.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "user-2", DietaryRestrictionId = 2 });
        ctx.SaveChanges();

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        var ids = await repo.GetSelectedIdsByUserIdAsync("user-1");

        Assert.That(ids, Is.EquivalentTo(new[] { 1 }));
    }

    // ── SetForUserAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task SetForUserAsync_AddsNewRestrictions()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        SeedRestrictions(ctx);

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        await repo.SetForUserAsync("user-1", [1, 2]);

        var ids = await CreateContext().UserDietaryRestrictions
            .Where(x => x.UserId == "user-1")
            .Select(x => x.DietaryRestrictionId)
            .ToListAsync();
        Assert.That(ids, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task SetForUserAsync_ReplacesExistingRestrictions()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        SeedRestrictions(ctx);
        ctx.UserDietaryRestrictions.Add(new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 1 });
        ctx.SaveChanges();

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        await repo.SetForUserAsync("user-1", [2, 3]);

        var ids = await CreateContext().UserDietaryRestrictions
            .Where(x => x.UserId == "user-1")
            .Select(x => x.DietaryRestrictionId)
            .ToListAsync();
        Assert.That(ids, Is.EquivalentTo(new[] { 2, 3 }));
        Assert.That(ids, Does.Not.Contain(1));
    }

    [Test]
    public async Task SetForUserAsync_ClearsAllRestrictionsWhenEmptyListPassed()
    {
        using var ctx = CreateContext();
        SeedUser(ctx);
        SeedRestrictions(ctx);
        ctx.UserDietaryRestrictions.AddRange(
            new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 1 },
            new UserDietaryRestriction { UserId = "user-1", DietaryRestrictionId = 2 });
        ctx.SaveChanges();

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        await repo.SetForUserAsync("user-1", []);

        var count = await CreateContext().UserDietaryRestrictions.CountAsync(x => x.UserId == "user-1");
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task SetForUserAsync_DoesNotAffectOtherUsers()
    {
        using var ctx = CreateContext();
        SeedUser(ctx, "user-1");
        SeedUser(ctx, "user-2");
        SeedRestrictions(ctx);
        ctx.UserDietaryRestrictions.Add(new UserDietaryRestriction { UserId = "user-2", DietaryRestrictionId = 3 });
        ctx.SaveChanges();

        var repo = new UserDietaryRestrictionRepository(CreateContext());
        await repo.SetForUserAsync("user-1", [1]);

        var user2Ids = await CreateContext().UserDietaryRestrictions
            .Where(x => x.UserId == "user-2")
            .Select(x => x.DietaryRestrictionId)
            .ToListAsync();
        Assert.That(user2Ids, Is.EquivalentTo(new[] { 3 }));
    }
}
