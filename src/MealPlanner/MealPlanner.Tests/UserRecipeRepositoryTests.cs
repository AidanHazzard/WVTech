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
    private List<Recipe> _recipes;
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

            _recipes =
            [
                new Recipe
                {
                    Id = 10,
                    Name = "Test1",
                    Directions = ""
                },

                new Recipe
                {
                    Id = 11,
                    Name = "Test2",
                    Directions = ""
                },

                new Recipe
                {
                    Id = 12,
                    Name = "Test3",
                    Directions = ""
                }
            ];

            _context.Add(_user);
            _context.AddRange(_recipes);

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

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
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
            Assert.That(result.First().Name, Is.EqualTo("Test1"));
        }
    }

    [Test]
    public async Task GetUserOwnedRecipesByUserId_ReturnsManyRecipes()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        _context.AddRange(
            new UserRecipe
            {
                User = _user,
                Recipe = _recipes[0],
                UserOwner = true
            },
            new UserRecipe
            {
                User = _user,
                Recipe = _recipes[1],
                UserOwner = true
            },
            new UserRecipe
            {
                User = _user,
                Recipe = _recipes[2],
                UserOwner = true
            }
        );

        _context.SaveChanges();

        // Act
        var result = await repo.GetUserOwnedRecipesByUserIdAsync(_user.Id);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
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

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(_recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(0f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns1_IfRecipeHas1UpVote()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true,
            UserVote = UserVoteType.UpVote
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(_recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(1f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_Returns0_IfRecipeHas1DownVote()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true,
            UserVote = UserVoteType.DownVote
        });
        _context.SaveChanges();

        // Act
        var result = await repo.GetRecipeVotePercentage(_recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(0f).Within(ERROR));
    }

    [Test]
    public async Task GetRecipeVotePercentage_ReturnsCorrectValue_IfRecipeHasMultipleVotes()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        User u1 = new User { FullName = "test1", Id = "1" };
        User u2 = new User { FullName = "test2", Id = "2" };
        User u3 = new User { FullName = "test3", Id = "3" };
        _context.AddRange(u1,u2,u3);

        _context.AddRange(
            new UserRecipe
            {
                User = _user,
                Recipe = _recipes[0],
                UserOwner = true
            },
            new UserRecipe
            {
                User = u1,
                Recipe = _recipes[0],
                UserVote = UserVoteType.UpVote
            },
            new UserRecipe
            {
                User = u2,
                Recipe = _recipes[0],
                UserVote = UserVoteType.UpVote
            },
            new UserRecipe
            {
                User = u3,
                Recipe = _recipes[0],
                UserVote = UserVoteType.DownVote
            }
        );
        _context.SaveChanges();
        // Act
        var result = await repo.GetRecipeVotePercentage(_recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(2/3f).Within(ERROR));
    }
    
    [Test]
    public async Task AddFavoriteAsync_CreatesNewUserRecipe()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        await repo.AddFavoriteAsync(_user, _recipes[0]);
        _context.SaveChanges();

        // Assert
        DbSet<UserRecipe> set = _context.Set<UserRecipe>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(set.Count(), Is.EqualTo(1));
            Assert.That(set.First().RecipeId, Is.EqualTo(_recipes[0].Id));
            Assert.That(set.First().UserId, Is.EqualTo(_user.Id));
            Assert.That(set.First().UserFavorite, Is.True);
        }
    }
    
    [Test]
    public async Task AddFavoriteAsync_UpdatesExistingUserRecipe()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe { User = _user, Recipe = _recipes[0] });
        _context.SaveChanges();

        // Act
        await repo.AddFavoriteAsync(_user, _recipes[0]);
        _context.SaveChanges();

        // Assert
        DbSet<UserRecipe> set = _context.Set<UserRecipe>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(set.Count(), Is.EqualTo(1));
            Assert.That(set.First().RecipeId, Is.EqualTo(_recipes[0].Id));
            Assert.That(set.First().UserId, Is.EqualTo(_user.Id));
            Assert.That(set.First().UserFavorite, Is.True);
        }
    }

    [Test]
    public async Task RemoveFavoriteAsync_MakesNoChange_IfNoRelationExists()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, _recipes[0].Id);
        int numChanges = _context.SaveChanges();

        // Assert
        Assert.That(numChanges, Is.Zero);
    }

    [Test]
    public async Task RemoveFavoriteAsync_RemovesFavorite()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);


        _context.Add(new UserRecipe 
        { 
            User = _user, 
            Recipe = _recipes[0], 
            UserOwner = true, 
            UserFavorite = true 
        });
        _context.SaveChanges();

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, _recipes[0].Id);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Find(_user.Id, _recipes[0].Id)!.UserFavorite, Is.False);
        }
    }

    [Test]
    public async Task RemoveFavoriteAsync_RemovesFavorite_AndRemovesRedundantEntry()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe { User = _user, Recipe = _recipes[0], UserFavorite = true });
        _context.SaveChanges();

        // Act
        await repo.RemoveFavoriteAsync(_user.Id, _recipes[0].Id);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Count(), Is.Zero);
        }
    }

    [Test]
    public async Task IsUserRecipeOwnerAsync_ReturnsFalse_IfRecipeNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });

        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwnerAsync(_user.Id, 11);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserRecipeOwnerAsync_ReturnsFalse_IfUserNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwnerAsync("abc", _recipes[0].Id);

        // Assert
        Assert.That(result, Is.False);
        
    }

    [Test]
    public async Task IsUserRecipeOwnerAsync_ReturnsFalse_IfUserNotOwner()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = false
        });
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwnerAsync(_user.Id, _recipes[0].Id);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserRecipeOwnerAsync_ReturnsTrue_IfUserOwner()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        bool result = await repo.IsUserRecipeOwnerAsync(_user.Id, _recipes[0].Id);

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

        

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserFavorite = true
        });
        _context.SaveChanges();
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync(_user.Id);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Id, Is.EqualTo(_recipes[0].Id));
    }

    [Test]
    public async Task GetFavoritesAsync_ReturnsMultipleFavorites()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        UserRecipe ur1 = new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserFavorite = true
        };


        UserRecipe ur2 = new UserRecipe
        {
            User = _user,
            Recipe = _recipes[1],
            UserFavorite = true
        };


        UserRecipe ur3 = new UserRecipe
        {
            User = _user,
            Recipe = _recipes[2],
            UserFavorite = true
        };

        _context.AddRange(ur1, ur2, ur3);
        _context.SaveChanges();
        
        // Act
        List<Recipe> result = await repo.GetFavoritesAsync(_user.Id);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task ChangeRecipeVoteAsync_CreatesNewEntry()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        await repo.ChangeRecipeVoteAsync(_user, _recipes[0], UserVoteType.UpVote);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task ChangeRecipeVoteAsync_UpdatesExistingEntry()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        await repo.ChangeRecipeVoteAsync(_user, _recipes[0], UserVoteType.UpVote);
        int numChanges = _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numChanges, Is.Not.Zero);
            Assert.That(_context.Set<UserRecipe>().Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task ChangeRecipeVoteAsync_DoesNotCreateNewEntry_IfVoteIsNoVote()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        // Act
        await repo.ChangeRecipeVoteAsync(_user, _recipes[0], UserVoteType.NoVote);
        _context.SaveChanges();

        // Assert
        Assert.That(_context.Set<UserRecipe>().Count(), Is.EqualTo(0));
    }

    [TestCase(UserVoteType.DownVote)]
    [TestCase(UserVoteType.UpVote)]
    [TestCase(UserVoteType.NoVote)]
    public async Task ChangeRecipeVoteAsync_ChangesToVote(UserVoteType voteType)
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserOwner = true
        });
        _context.SaveChanges();

        // Act
        await repo.ChangeRecipeVoteAsync(_user, _recipes[0], voteType);
        _context.SaveChanges();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Set<UserRecipe>().Count(), Is.EqualTo(1));
            Assert.That(_context.Set<UserRecipe>().First().UserVote, Is.EqualTo(voteType));
        }
    }

    [Test]
    public async Task ChangeRecipeVoteAsync_ChangesToNoVote_AndRemovesRedundantEntry()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserVote = UserVoteType.DownVote
        });
        _context.SaveChanges();

        // Act
        await repo.ChangeRecipeVoteAsync(_user, _recipes[0], UserVoteType.NoVote);
        _context.SaveChanges();

        // Assert
        Assert.That(_context.Set<UserRecipe>(), Is.Empty);
        
    }

    [TestCase(UserVoteType.DownVote)]
    [TestCase(UserVoteType.UpVote)]
    [TestCase(UserVoteType.NoVote)]
    public async Task GetUserRecipeVoteAsync_ReturnsCorrectVote(UserVoteType userVote)
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);

        _context.Add(new UserRecipe
        {
            User = _user,
            Recipe = _recipes[0],
            UserVote = userVote
        });
        _context.SaveChanges();

        // Act
        UserVoteType result = await repo.GetUserRecipeVoteAsync(_user.Id, _recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(userVote));
    }

    
    [TestCase("ABCD", 0)]
    [TestCase("", 10)]
    public async Task GetUserRecipeVoteAsync_ReturnsNoVote_IfUserOrRecipeNotFound(string userId, int recipeId)
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        // Act
        UserVoteType result = await repo.GetUserRecipeVoteAsync(userId, recipeId);

        // Assert
        Assert.That(result, Is.EqualTo(UserVoteType.NoVote));
    }

    [Test]
    public async Task GetUserRecipeVoteAsync_ReturnsNoVote_IfUserRecipeNotFound()
    {
        // Arrange
        UserRecipeRepository repo = new UserRecipeRepository(_context);
        
        // Act
        UserVoteType result = await repo.GetUserRecipeVoteAsync(_user.Id, _recipes[0].Id);

        // Assert
        Assert.That(result, Is.EqualTo(UserVoteType.NoVote));
    }
}