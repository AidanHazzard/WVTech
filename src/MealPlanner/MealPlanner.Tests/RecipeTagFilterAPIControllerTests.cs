using System.Data.Common;
using System.Security.Claims;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
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
public class RecipeTagFilterAPIControllerTests
{
    private DbConnection _connection;
    private MealPlannerDBContext _context;

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

    private RecipeAPIController CreateController(
        IRecipeRepository? recipeRepo = null,
        IUserRecipeRepository? urRepo = null,
        ITagRepository? tagRepo = null,
        IExternalRecipeService? externalService = null)
    {
        return new RecipeAPIController(
            _context,
            recipeRepo ?? new Mock<IRecipeRepository>().Object,
            urRepo ?? new Mock<IUserRecipeRepository>().Object,
            new Mock<IRegistrationService>().Object,
            externalService,
            tagRepo
        );
    }

    // --- SearchRecipesByName with tag ---

    [Test]
    public async Task SearchRecipesByName_WithTag_ReturnsStatus200OK()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByNameAndTag("", "Breakfast"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Porridge", Directions = "" }]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        var result = await controller.SearchRecipesByName("", tag: "Breakfast");

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task SearchRecipesByName_WithTag_Returns404NotFound_WhenNoMatch()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByNameAndTag("", "Breakfast")).Returns([]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        var result = await controller.SearchRecipesByName("", tag: "Breakfast");

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>().Or.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task SearchRecipesByName_WithTag_ReturnsCorrectRecipeName()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByNameAndTag("Oat", "Breakfast"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Bowl", Directions = "" }]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        var result = (await controller.SearchRecipesByName("Oat", tag: "Breakfast")) as OkObjectResult;
        var recipes = result?.Value as IEnumerable<RecipeDTO>;

        // Assert
        Assert.That(recipes?.First().Name, Is.EqualTo("Oatmeal Bowl"));
    }

    [Test]
    public async Task SearchRecipesByName_WithTag_DoesNotCallExternalService()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByNameAndTag("", "Breakfast"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Porridge", Directions = "" }]);
        var externalService = new Mock<IExternalRecipeService>();
        var controller = CreateController(recipeRepo: repo.Object, externalService: externalService.Object);

        // Act
        await controller.SearchRecipesByName("", tag: "Breakfast");

        // Assert — Edamam API must never be called when tag filtering is active
        externalService.Verify(
            s => s.SearchExternalRecipesByName(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task SearchRecipesByName_WithTag_UsesGetRecipesByNameAndTag_NotGetRecipesByName()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByNameAndTag("oat", "Breakfast"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Bowl", Directions = "" }]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        await controller.SearchRecipesByName("oat", tag: "Breakfast");

        // Assert — tag path must call the tag-aware method, not the name-only method
        repo.Verify(r => r.GetRecipesByNameAndTag("oat", "Breakfast"), Times.Once);
        repo.Verify(r => r.GetRecipesByName(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SearchRecipesByName_WithoutTag_StillUsesGetRecipesByName()
    {
        // Arrange
        var repo = new Mock<IRecipeRepository>();
        repo.Setup(r => r.GetRecipesByName("oat"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Cookies", Directions = "" }]);
        var controller = CreateController(recipeRepo: repo.Object);

        // Act
        await controller.SearchRecipesByName("oat");

        // Assert — existing name-only path is unchanged when no tag is provided
        repo.Verify(r => r.GetRecipesByName("oat"), Times.Once);
        repo.Verify(r => r.GetRecipesByNameAndTag(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // --- GetRecipeTags ---

    [Test]
    public async Task GetRecipeTags_ReturnsStatus200OK_WithTagNames()
    {
        // Arrange
        var tagRepo = new Mock<ITagRepository>();
        tagRepo.Setup(r => r.GetTagsByPopularityAsync())
            .ReturnsAsync(
            [
                new Tag { Id = 1, Name = "Breakfast" },
                new Tag { Id = 2, Name = "Dinner" }
            ]);
        var controller = CreateController(tagRepo: tagRepo.Object);

        // Act
        var result = await controller.GetRecipeTags();

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        var tags = ok?.Value as IEnumerable<string>;
        Assert.That(tags, Is.EquivalentTo(new[] { "Breakfast", "Dinner" }));
    }

    [Test]
    public async Task GetRecipeTags_ReturnsEmptyList_WhenTagRepositoryIsNull()
    {
        // Arrange — no tag repository injected (simulates NoApi mode or unconfigured DI)
        var controller = CreateController();

        // Act
        var result = await controller.GetRecipeTags();

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        var tags = ok?.Value as IEnumerable<string>;
        Assert.That(tags, Is.Empty);
    }

    [Test]
    public async Task GetRecipeTags_ReturnsEmptyList_WhenNoTagsExist()
    {
        // Arrange
        var tagRepo = new Mock<ITagRepository>();
        tagRepo.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync([]);
        var controller = CreateController(tagRepo: tagRepo.Object);

        // Act
        var result = await controller.GetRecipeTags();

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        var tags = ok?.Value as IEnumerable<string>;
        Assert.That(tags, Is.Empty);
    }
}
