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
public class DietaryRestrictionFilterRepositoryTests
{
    private DbConnection _connection;
    private DbContextOptions<MealPlannerDBContext> _contextOptions;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new MealPlannerDBContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    MealPlannerDBContext CreateContext() => new MealPlannerDBContext(_contextOptions);

    [TearDown]
    public void TearDown() => _connection.Dispose();

    [Test]
    public void GetRecipesByNameAndRestrictions_ReturnsOnlyRecipesWithAllRequiredTags()
    {
        using var context = CreateContext();
        var nutAllergyTag = new Tag { Name = "Nut Allergy" };
        context.Tags.Add(nutAllergyTag);
        context.Recipes.Add(new Recipe { Name = "Fruit Salad", Directions = "", Tags = [nutAllergyTag] });
        context.Recipes.Add(new Recipe { Name = "Peanut Butter Cookies", Directions = "" });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        var results = repo.GetRecipesByNameAndRestrictions("", ["Nut Allergy"]);

        Assert.That(results.Select(r => r.Name), Is.EquivalentTo(new[] { "Fruit Salad" }));
    }

    [Test]
    public void GetRecipesByNameAndRestrictions_ExcludesRecipeMissingOneRequiredTag()
    {
        using var context = CreateContext();
        var nutTag = new Tag { Name = "Nut Allergy" };
        var glutenTag = new Tag { Name = "Gluten-Free" };
        context.Tags.AddRange(nutTag, glutenTag);
        context.Recipes.Add(new Recipe { Name = "Rice Bowl", Directions = "", Tags = [nutTag, glutenTag] });
        context.Recipes.Add(new Recipe { Name = "Nut-Free Bread", Directions = "", Tags = [nutTag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        var results = repo.GetRecipesByNameAndRestrictions("", ["Nut Allergy", "Gluten-Free"]);

        Assert.That(results.Select(r => r.Name), Is.EquivalentTo(new[] { "Rice Bowl" }));
    }

    [Test]
    public void GetRecipesByNameAndRestrictions_FiltersOnNameWhenProvided()
    {
        using var context = CreateContext();
        var nutTag = new Tag { Name = "Nut Allergy" };
        context.Tags.Add(nutTag);
        context.Recipes.Add(new Recipe { Name = "Fruit Salad", Directions = "", Tags = [nutTag] });
        context.Recipes.Add(new Recipe { Name = "Oatmeal Cookies", Directions = "", Tags = [nutTag] });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        var results = repo.GetRecipesByNameAndRestrictions("Fruit", ["Nut Allergy"]);

        Assert.That(results.Select(r => r.Name), Is.EquivalentTo(new[] { "Fruit Salad" }));
    }

    [Test]
    public void GetRecipesByNameAndRestrictions_ReturnsEmpty_WhenNoRecipesPassAllRestrictions()
    {
        using var context = CreateContext();
        context.Recipes.Add(new Recipe { Name = "Peanut Butter Cookies", Directions = "" });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        var results = repo.GetRecipesByNameAndRestrictions("", ["Nut Allergy"]);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetRecipesByNameAndRestrictions_ExcludesExternalRecipes()
    {
        using var context = CreateContext();
        var nutTag = new Tag { Name = "Nut Allergy" };
        context.Tags.Add(nutTag);
        context.Recipes.Add(new Recipe
        {
            Name = "External Fruit Salad",
            Directions = "",
            ExternalUri = "https://example.com/recipe",
            Tags = [nutTag]
        });
        context.SaveChanges();
        var repo = new RecipeRepository(CreateContext());

        var results = repo.GetRecipesByNameAndRestrictions("", ["Nut Allergy"]);

        Assert.That(results, Is.Empty);
    }
}

[TestFixture]
public class DietaryRestrictionFilterControllerTests
{
    private Mock<IRecipeRepository> _recipeRepo;
    private Mock<IUserRecipeRepository> _urRepo;
    private Mock<IRegistrationService> _registrationService;
    private Mock<IUserDietaryRestrictionRepository> _dietaryRepo;

    [SetUp]
    public void SetUp()
    {
        _recipeRepo = new Mock<IRecipeRepository>();
        _urRepo = new Mock<IUserRecipeRepository>();
        _registrationService = new Mock<IRegistrationService>();
        _dietaryRepo = new Mock<IUserDietaryRestrictionRepository>();
    }

    private RecipeAPIController CreateController(bool authenticated = true)
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<MealPlannerDBContext>().UseSqlite(connection).Options;
        var context = new MealPlannerDBContext(options);

        var controller = new RecipeAPIController(
            context,
            _recipeRepo.Object,
            _urRepo.Object,
            _registrationService.Object,
            userDietaryRestrictionRepo: _dietaryRepo.Object
        );

        if (authenticated)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "gary@fakeemail.com") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }
        else
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        return controller;
    }

    [Test]
    public async Task SearchRecipesByName_WhenUserHasRestrictions_CallsGetRecipesByNameAndRestrictions()
    {
        var user = new User { Id = "user-1", Email = "gary@fakeemail.com" };
        _registrationService.Setup(s => s.FindUserByClaimAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var restriction = new DietaryRestriction { Id = 1, Name = "Nut Allergy" };
        _dietaryRepo.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync([new UserDietaryRestriction
            {
                UserId = "user-1",
                DietaryRestrictionId = 1,
                DietaryRestriction = restriction
            }]);

        _recipeRepo.Setup(r => r.GetRecipesByNameAndRestrictions("Cookies", It.IsAny<IEnumerable<string>>()))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Cookies", Directions = "", Tags = [new Tag { Name = "Nut Allergy" }] }]);

        var controller = CreateController(authenticated: true);

        await controller.SearchRecipesByName("Cookies");

        _recipeRepo.Verify(r => r.GetRecipesByNameAndRestrictions("Cookies", It.Is<IEnumerable<string>>(
            names => names.Contains("Nut Allergy"))), Times.Once);
        _recipeRepo.Verify(r => r.GetRecipesByName(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SearchRecipesByName_WhenUserHasRestrictions_SetsMatchedRestrictionTagsOnDTO()
    {
        var user = new User { Id = "user-1", Email = "gary@fakeemail.com" };
        _registrationService.Setup(s => s.FindUserByClaimAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var restriction = new DietaryRestriction { Id = 1, Name = "Nut Allergy" };
        _dietaryRepo.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync([new UserDietaryRestriction
            {
                UserId = "user-1",
                DietaryRestrictionId = 1,
                DietaryRestriction = restriction
            }]);

        _recipeRepo.Setup(r => r.GetRecipesByNameAndRestrictions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns([new Recipe { Id = 1, Name = "Fruit Salad", Directions = "", Tags = [new Tag { Name = "Nut Allergy" }] }]);

        var controller = CreateController(authenticated: true);

        var result = (await controller.SearchRecipesByName("Fruit")) as OkObjectResult;
        var recipes = (result?.Value as IEnumerable<RecipeDTO>)?.ToList();

        Assert.That(recipes, Has.Count.EqualTo(1));
        Assert.That(recipes![0].MatchedRestrictionTags, Contains.Item("Nut Allergy"));
    }

    [Test]
    public async Task SearchRecipesByName_WhenUserHasNoRestrictions_UsesOriginalNameSearch()
    {
        var user = new User { Id = "user-1", Email = "gary@fakeemail.com" };
        _registrationService.Setup(s => s.FindUserByClaimAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _dietaryRepo.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync([]);

        _recipeRepo.Setup(r => r.GetRecipesByName("Cookies"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Cookies", Directions = "" }]);

        var controller = CreateController(authenticated: true);

        await controller.SearchRecipesByName("Cookies");

        _recipeRepo.Verify(r => r.GetRecipesByName("Cookies"), Times.Once);
        _recipeRepo.Verify(r => r.GetRecipesByNameAndRestrictions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Test]
    public async Task SearchRecipesByName_WhenUserNotAuthenticated_UsesOriginalNameSearch()
    {
        _recipeRepo.Setup(r => r.GetRecipesByName("Cookies"))
            .Returns([new Recipe { Id = 1, Name = "Oatmeal Cookies", Directions = "" }]);

        var controller = CreateController(authenticated: false);

        await controller.SearchRecipesByName("Cookies");

        _recipeRepo.Verify(r => r.GetRecipesByName("Cookies"), Times.Once);
        _recipeRepo.Verify(r => r.GetRecipesByNameAndRestrictions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}
