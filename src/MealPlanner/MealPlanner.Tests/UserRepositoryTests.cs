using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class UserRepositoryTests
{
    private DbConnection _connection;
    private DbContextOptions<MealPlannerDBContext> _contextOptions;
    private MealPlannerDBContext _context;
    private User _user;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(_connection)
            .Options;
        
        _context = new MealPlannerDBContext(_contextOptions);

        if (_context.Database.EnsureCreated())
        {
            _user = new User
            {
                Id = "ABCD",
                FullName = ""
            };

            _context.Add(_user);

            _context.SaveChanges();
        }
    }

    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task GetUserOwnedRecipesByUserId_ReturnsOneRecipe()
    {
        // Arrange
        UserRepository repo = new UserRepository(_context);

        _context.Add( new Recipe
        {
            Id = 10,
            Name = "Test",
            Directions = "",
            Owner = _user
        });

        _context.SaveChanges();

        // Act
        var result = await repo.GetUserOwnedRecipesByUserIdAsync(_user.Id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(10));
            Assert.That(result.First().Name, Is.EqualTo("Test"));
            Assert.That(result.First().Owner.Id, Is.EqualTo(_user.Id));
        }
    }

    [Test]
    public async Task GetUserOwnedRecipesByUserId_ReturnsManyRecipes()
    {
        // Arrange
        UserRepository repo = new UserRepository(_context);

        _context.AddRange([
            new Recipe
            {
                Id = 10,
                Name = "Test1",
                Directions = "",
                Owner = _user
            },
            new Recipe
            {
                Id = 11,
                Name = "Test2",
                Directions = "",
                Owner = _user
            },
            new Recipe
            {
                Id = 12,
                Name = "Test3",
                Directions = "",
                Owner = _user
            }
        ]);

        _context.SaveChanges();

        // Act
        var result = await repo.GetUserOwnedRecipesByUserIdAsync(_user.Id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Id, Is.EqualTo(10));
            Assert.That(result[1].Id, Is.EqualTo(11));
            Assert.That(result[2].Id, Is.EqualTo(12));
        }
    }

    [Test]
    public async Task GetUserOwnedRecipesByUserId_ReturnsNoRecipes()
    {
        // Arrange
        UserRepository repo = new UserRepository(_context);

        // Act
        var result = await repo.GetUserOwnedRecipesByUserIdAsync(_user.Id);

        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }
}