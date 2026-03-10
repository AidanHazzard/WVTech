using System.Data.Common;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class UserRecipeRepositoryTests
{
    private DbConnection _connection;
    private DbContextOptions<MealPlannerDBContext> _contextOptions;
    private MealPlannerDBContext _context;
    private User _user;
    private const float ERROR = 0.005f;

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
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        Recipe recipe = new Recipe
        {
            Id = 10,
            Name = "Test",
            Directions = ""
        };
        _context.Add(recipe);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = recipe,
            UserOwner = true
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
        }
    }

    [Test]
    public async Task GetUserOwnedRecipesByUserId_ReturnsManyRecipes()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        Recipe r2 = new Recipe
        {
            Id = 11,
            Name = "Test2",
            Directions = ""
        };

        Recipe r3 = new Recipe
        {
            Id = 12,
            Name = "Test3",
            Directions = ""
        };

        _context.AddRange([r1, r2, r3]);

        _context.AddRange([
            new UserRecipe
            {
                User = _user,
                Recipe = r1,
                UserOwner = true
            },
            new UserRecipe
            {
                User = _user,
                Recipe = r2,
                UserOwner = true
            },
            new UserRecipe
            {
                User = _user,
                Recipe = r3,
                UserOwner = true
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
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        var result = await repo.GetUserOwnedRecipesByUserIdAsync(_user.Id);

        // Assert
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns0_IfRecipeIdNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        var result = await repo.GetRecipeVotePercentage(10);

        // Assert
        Assert.That(result, Is.EqualTo(0f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns0_IfRecipeHasNoVotes()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        _context.Add(r1);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(10);

        // Assert
        Assert.That(result, Is.EqualTo(0f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns1_IfRecipeHas1UpVote()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        _context.Add(r1);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true,
            UserVote = UserVoteType.UpVote
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(10);

        // Assert
        Assert.That(result, Is.EqualTo(1f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns0_IfRecipeHas1DownVote()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        _context.Add(r1);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true,
            UserVote = UserVoteType.DownVote
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(10);

        // Assert
        Assert.That(result, Is.EqualTo(0f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_ReturnsCorrectValue_IfRecipeHasMultipleVotes()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        _context.Add(r1);

        User u1 = new User { FullName = "test1", Id = "1" };
        User u2 = new User { FullName = "test2", Id = "2" };
        User u3 = new User { FullName = "test3", Id = "3" };
        _context.AddRange(u1,u2,u3);

        _context.AddRange(
            new UserRecipe
            {
                User = _user,
                Recipe = r1,
                UserOwner = true
            },
            new UserRecipe
            {
                User = u1,
                Recipe = r1,
                UserVote = UserVoteType.UpVote
            },
            new UserRecipe
            {
                User = u2,
                Recipe = r1,
                UserVote = UserVoteType.UpVote
            },
            new UserRecipe
            {
                User = u3,
                Recipe = r1,
                UserVote = UserVoteType.DownVote
            }
        );
        _context.SaveChanges();
        // Act
        var result = await repo.GetRecipeVotePercentage(10);

        // Assert
        Assert.That(result, Is.EqualTo(2/3f).Within(ERROR));
    }
    
    [Test]
    public async Task AddFavoriteAsync_CreatesNewUserRecipe()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        _context.Add(r1);
        _context.SaveChanges();

        // Act
        await repo.AddFavoriteAsync(_user, r1);
        _context.SaveChanges();

        // Assert
        DbSet<UserRecipe> set = _context.Set<UserRecipe>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(set.Count(), Is.EqualTo(1));
            Assert.That(set.First().RecipeId, Is.EqualTo(r1.Id));
            Assert.That(set.First().UserId, Is.EqualTo(_user.Id));
            Assert.That(set.First().UserFavorite, Is.True);
        }
    }
    
    [Test]
    public async Task AddFavoriteAsync_UpdatesExistingUserRecipe()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        _context.AddRange(r1, new UserRecipe { User = _user, Recipe = r1 });
        _context.SaveChanges();

        // Act
        await repo.AddFavoriteAsync(_user, r1);
        _context.SaveChanges();

        // Assert
        DbSet<UserRecipe> set = _context.Set<UserRecipe>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(set.Count(), Is.EqualTo(1));
            Assert.That(set.First().RecipeId, Is.EqualTo(r1.Id));
            Assert.That(set.First().UserId, Is.EqualTo(_user.Id));
            Assert.That(set.First().UserFavorite, Is.True);
        }
    }

    [Test]
    public async Task RemoveFavoriteAsync_MakesNoChange_IfNoRelationExists()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        _context.AddRange(r1);
        _context.SaveChanges();

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, r1.Id);
        int numChanges = _context.SaveChanges();

        // Assert
        Assert.That(numChanges, Is.Zero);
    }

    [Test]
    public async Task RemoveFavoriteAsync_RemovesFavorite()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        _context.AddRange(r1, new UserRecipe { User = _user, Recipe = r1, UserOwner = true, UserFavorite = true });
        _context.SaveChanges();

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, r1.Id);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Find(_user.Id, r1.Id)!.UserFavorite, Is.False);
        }
    }

    [Test]
    public async Task RemoveFavoriteAsync_RemovesFavorite_AndRemovesRedundantEntry()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        _context.AddRange(r1, new UserRecipe { User = _user, Recipe = r1, UserFavorite = true });
        _context.SaveChanges();

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, r1.Id);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Count(), Is.Zero);
        }
    }

    [Test]
    public async Task IsUserRecipeOwner_ReturnsFalse_IfRecipeNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true
        };

        _context.AddRange(r1, ur);
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwner(_user.Id, 11);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserRecipeOwner_ReturnsFalse_IfUserNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true
        };

        _context.AddRange(r1, ur);
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwner("abc", r1.Id);

        // Assert
        Assert.That(result, Is.False);
        
    }

    [Test]
    public async Task IsUserRecipeOwner_ReturnsFalse_IfUserNotOwner()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = false
        };

        _context.AddRange(r1, ur);
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwner(_user.Id, r1.Id);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserRecipeOwner_ReturnsTrue_IfUserOwner()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserOwner = true
        };

        _context.AddRange(r1, ur);
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwner(_user.Id, r1.Id);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsEmptyList_IfUserNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync("fail");

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsEmptyList_IfNoFavorites()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync(_user.Id);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetFavoritesAsync_Returns1Favorite()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserFavorite = true
        };

        _context.AddRange(r1, ur);
        _context.SaveChanges();
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync(_user.Id);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Id, Is.EqualTo(r1.Id));
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsMultipleFavorites()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "Test1",
            Directions = ""
        };
        
        Recipe r2 = new Recipe
        {
            Id = 11,
            Name = "Test1",
            Directions = ""
        };

        
        Recipe r3 = new Recipe
        {
            Id = 12,
            Name = "Test1",
            Directions = ""
        };

        UserRecipe ur1 = new UserRecipe
        {
            User = _user,
            Recipe = r1,
            UserFavorite = true
        };


        UserRecipe ur2 = new UserRecipe
        {
            User = _user,
            Recipe = r2,
            UserFavorite = true
        };


        UserRecipe ur3 = new UserRecipe
        {
            User = _user,
            Recipe = r3,
            UserFavorite = true
        };

        _context.AddRange(r1, r2, r3, ur1, ur2, ur3);
        _context.SaveChanges();
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync(_user.Id);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count(), Is.EqualTo(3));
    }
}