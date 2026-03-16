using System.Data.Common;
using System.Security.Claims;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using MealPlanner.Models.DTO;
using MealPlanner.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class RecipeAPIControllerTests
{
    private DbConnection _connection;
    private MealPlannerDBContext _context;
    private const float ERROR = 0.005f;

    [SetUp]
    public void SetUp()
    {
        
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(_connection)
            .Options;
        
        _context = new MealPlannerDBContext(contextOptions);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private RecipeAPIController CreateController(IRecipeRepository? recipeRepo=null, IUserRecipeRepository? urRepo=null, IRegistrationService? regService=null)
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Name, "testuser")
            ],
            authenticationType: "TestAuth"));
        
        var mockRegService = new Mock<IRegistrationService>();
        mockRegService.Setup(rs => rs.FindUserByClaimAsync(user)).ReturnsAsync(
            new User
            {
                FullName = "testuser",
                Id = "ABCD"
            }
        );

        RecipeAPIController controller = new RecipeAPIController
        (
            _context,
            recipeRepo ?? new Mock<IRecipeRepository>().Object,
            urRepo ?? new Mock<IUserRecipeRepository>().Object,
            regService ?? mockRegService.Object
        );
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Test]
    public async Task SearchRecipesByName_ReturnsStatus200OK()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([new Recipe{Id=1, Name = "Oatmeal Cookies", Directions = ""}]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        var result = await controller.SearchRecipesByName(searchTerm);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task SearchRecipesByName_ReturnsStatus404NotFound()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([]);
        var controller = CreateController(recipeRepo: repo.Object);
        
        // Act
        var result = await controller.SearchRecipesByName(searchTerm);

        // Assert

        // If we want to add an error message later, the class that would return inherits from object result
        // rather than status code result, so testing for both for potential future proofing
        Assert.That(result, Is.TypeOf<NotFoundResult>().Or.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task SearchRecipesByName_ReturnsOneRecipe()
    {
        // Arrange
        string searchTerm = "oat";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns([new Recipe{Id=1, Name = "Oatmeal Cookies", Directions = ""}]);
        var controller = CreateController(recipeRepo: repo.Object);
        
        // Act
        var result = (await controller.SearchRecipesByName(searchTerm)) as OkObjectResult;
        var recipe = result?.Value as IEnumerable<RecipeDTO>;

        // Assert
        Assert.That(recipe?.First().Name, Is.EqualTo("Oatmeal Cookies"));
    }

    [Test]
    public async Task SearchRecipesByNameReturns_MultipleRecipes()
    {
        // Arrange
        string searchTerm = "spagh";
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(repo => repo.GetRecipesByName(searchTerm)).Returns(
            [
                new Recipe { Name="Spaghetti All'assassina", Directions=""},
                new Recipe { Name="Spaghetti and Meatballs", Directions=""},
                new Recipe { Name="Vegan Spaghetti with Mushrooms", Directions=""},
                new Recipe { Name="Baked Spaghetti Casserole", Directions=""}
            ]
        );
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        var result = (await controller.SearchRecipesByName(searchTerm)) as OkObjectResult;
        var recipes = result?.Value as IEnumerable<RecipeDTO>;

        // Assert
        Assert.That(recipes?.Count(), Is.EqualTo(4));
    }

    [Test]
    public async Task ChangeRecipeVote_Returns403Forbidden_IfUserClaimInvalid()
    {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext();

        // Act
        var result = await controller.ChangeRecipeVote(1, UserVoteType.UpVote);

        // Assert
        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task ChangeRecipeVote_Returns404NotFound_IfRecipeIdInvalid()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.ChangeRecipeVote(0, UserVoteType.UpVote);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [TestCase(UserVoteType.UpVote)]
    [TestCase(UserVoteType.DownVote)]
    [TestCase(UserVoteType.NoVote)]
    public async Task ChangeRecipeVote_Returns202Accepted_IfRequestValid(UserVoteType voteType)
    {
        // Arrange
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "test",
            Directions = ""
        };
        var mockRecipeRepo = new Mock<IRecipeRepository>();
        mockRecipeRepo.Setup(r => r.Read(r1.Id)).Returns(r1);
        var controller = CreateController(recipeRepo: mockRecipeRepo.Object);
        
        // Act
        var result = await controller.ChangeRecipeVote(r1.Id, voteType);

        // Assert
        Assert.That(result, Is.TypeOf<AcceptedResult>());
    }

    [Test]
    public async Task GetRecipeRating_Returns404NotFound_IfRecipeIdInvalid()
    {
        // Arrange
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "test",
            Directions = ""
        };
        var controller = CreateController();
        
        // Act
        var result = await controller.GetRecipeRating(r1.Id);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetRecipeRating_Returns200Ok_AndRecipeRating()
    {
        
        // Arrange
        Recipe r1 = new Recipe
        {
            Id = 10,
            Name = "test",
            Directions = ""
        };
        var mockRecipeRepo = new Mock<IRecipeRepository>();
        mockRecipeRepo.Setup(r => r.Read(r1.Id)).Returns(r1);

        var mockUrRepo = new Mock<IUserRecipeRepository>();
        mockUrRepo.Setup(ur => ur.GetRecipeVotePercentage(r1.Id)).ReturnsAsync(0.75f);
        var controller = CreateController(recipeRepo: mockRecipeRepo.Object, urRepo: mockUrRepo.Object);
        
        // Act
        var result = await controller.GetRecipeRating(r1.Id);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok.Value as float?, Is.EqualTo(0.75f).Within(ERROR));
    }
}