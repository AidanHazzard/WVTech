using System.Security.Claims;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class WVT144Tests
{
    private MealController _controller;
    private MealPlannerDBContext _context;
    private ClaimsPrincipal _user;

    private Mock<IMealRepository> _mealRepoMock;
    private Mock<IRecipeRepository> _recipeRepoMock;
    private Mock<IRegistrationService> _registrationServiceMock;
    private Mock<IMealRecommendationService> _reccServiceMock;
    private Mock<ITagRepository> _tagRepoMock;

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        _context = new MealPlannerDBContext(
            new DbContextOptionsBuilder<MealPlannerDBContext>()
                .UseSqlite(connection)
                .Options);
        _context.Database.EnsureCreated();

        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Name, "testuser")
            },
            "TestAuth"));

        _registrationServiceMock = new Mock<IRegistrationService>();
        _registrationServiceMock
            .Setup(r => r.FindUserByClaimAsync(_user))
            .ReturnsAsync(new User { Id = "user-1", FullName = "testuser" });

        _recipeRepoMock = new Mock<IRecipeRepository>();
        _mealRepoMock = new Mock<IMealRepository>();
        _reccServiceMock = new Mock<IMealRecommendationService>();
        _tagRepoMock = new Mock<ITagRepository>();
        _tagRepoMock.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync([]);

        _mealRepoMock
            .Setup(r => r.GetUserMealsByDateAsync(It.IsAny<User>(), It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        _controller = new MealController(
            _registrationServiceMock.Object,
            _recipeRepoMock.Object,
            _mealRepoMock.Object,
            _context,
            _tagRepoMock.Object,
            _reccServiceMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _user }
        };

        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    // MealSize enum

    [Test]
    public void MealSize_SnackSizes_AreIdentifiedAsSnacks()
    {
        Assert.That(MealSize.SmallSnack.IsSnack(), Is.True);
        Assert.That(MealSize.LargeSnack.IsSnack(), Is.True);
    }

    [Test]
    public void MealSize_MealSizes_AreNotIdentifiedAsSnacks()
    {
        Assert.That(MealSize.Small.IsSnack(), Is.False);
        Assert.That(MealSize.Average.IsSnack(), Is.False);
        Assert.That(MealSize.Large.IsSnack(), Is.False);
    }

    // Controller — NewMeal GET

    [Test]
    public async Task NewMeal_Get_PassesAvailableTagsToViewBag()
    {
        var tags = new List<Tag> { new Tag { Id = 1, Name = "Vegan" } };
        _tagRepoMock.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync(tags);

        var result = (ViewResult)await _controller.NewMeal((string?)null);

        Assert.That(_controller.ViewBag.AvailableTags, Is.Not.Null);
        Assert.That(((List<Tag>)_controller.ViewBag.AvailableTags).Count, Is.EqualTo(1));
    }

    // Controller — GenerateDayPlan POST

    [Test]
    public async Task GenerateDayPlan_Post_CallsRecommendationServiceWithConfig()
    {
        var config = new DayPlanConfigViewModel
        {
            MealCount = 3,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences =
            [
                new MealPreferenceViewModel { Size = MealSize.Average },
                new MealPreferenceViewModel { Size = MealSize.Small },
                new MealPreferenceViewModel { Size = MealSize.Large }
            ]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        _reccServiceMock.Verify(
            s => s.GetRecommendedMealsForUser(
                It.IsAny<User>(), It.IsAny<DateTime>(), config),
            Times.Once);
    }

    [Test]
    public async Task GenerateDayPlan_Post_RedirectsToDayPlanSummary()
    {
        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average }]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        var result = await _controller.GenerateDayPlan(config);

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("DayPlanSummary"));
    }

    // Controller — DayPlanSummary GET

    [Test]
    public async Task DayPlanSummary_WithNoTempData_RedirectsToHomeIndex()
    {
        var result = await _controller.DayPlanSummary(DateTime.Today.ToString("yyyy-MM-dd"));

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        var redirect = (RedirectToActionResult)result;
        Assert.That(redirect.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
    }

    [Test]
    public async Task DayPlanSummary_WithTempData_CallsGetMealsByIdsAsync()
    {
        _controller.TempData["GeneratedMealIds"] = "10,20";
        _mealRepoMock
            .Setup(r => r.GetMealsByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([]);
        _tagRepoMock.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync([]);

        await _controller.DayPlanSummary(DateTime.Today.ToString("yyyy-MM-dd"));

        _mealRepoMock.Verify(
            r => r.GetMealsByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 10, 20 }))),
            Times.Once);
    }

    [Test]
    public async Task DayPlanSummary_WithTempData_IncludesAvailableTagsInViewModel()
    {
        _controller.TempData["GeneratedMealIds"] = "1";
        var tags = new List<Tag> { new Tag { Id = 3, Name = "Vegan" } };
        _tagRepoMock.Setup(r => r.GetTagsByPopularityAsync()).ReturnsAsync(tags);
        _mealRepoMock
            .Setup(r => r.GetMealsByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([]);

        var result = (ViewResult)await _controller.DayPlanSummary(DateTime.Today.ToString("yyyy-MM-dd"));
        var model = (DayPlanSummaryViewModel)result.Model!;

        Assert.That(model.AvailableTags, Is.Not.Empty);
        Assert.That(model.AvailableTags.First().Name, Is.EqualTo("Vegan"));
    }

    // Controller — custom tag resolution

    [Test]
    public async Task GenerateDayPlan_Post_WithCustomTagName_ResolvesTagAndAddsToPreferences()
    {
        var tag = new Tag { Id = 42, Name = "Vegan" };
        _tagRepoMock.Setup(r => r.FindByNameAsync("Vegan")).ReturnsAsync(tag);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average, CustomTagName = "Vegan" }]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        Assert.That(config.MealPreferences[0].TagIds, Does.Contain(42));
    }

    [Test]
    public async Task GenerateDayPlan_Post_WithUnknownCustomTagName_DoesNotAddTag()
    {
        _tagRepoMock.Setup(r => r.FindByNameAsync("Unknown")).ReturnsAsync((Tag?)null);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average, CustomTagName = "Unknown" }]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        Assert.That(config.MealPreferences[0].TagIds, Is.Empty);
    }

    [Test]
    public async Task GenerateDayPlan_Post_WithMatchingTitle_AddsMatchingTagToPreferences()
    {
        var tag = new Tag { Id = 7, Name = "Vegan" };
        _tagRepoMock.Setup(r => r.FindByNameAsync("Vegan")).ReturnsAsync(tag);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average, Title = "Vegan" }]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        Assert.That(config.MealPreferences[0].TagIds, Does.Contain(7));
    }

    [Test]
    public async Task GenerateDayPlan_Post_WithNonMatchingTitle_DoesNotAddTag()
    {
        _tagRepoMock.Setup(r => r.FindByNameAsync("Weekend Brunch")).ReturnsAsync((Tag?)null);

        var config = new DayPlanConfigViewModel
        {
            MealCount = 1,
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day,
            MealPreferences = [new MealPreferenceViewModel { Size = MealSize.Average, Title = "Weekend Brunch" }]
        };

        _reccServiceMock
            .Setup(s => s.GetRecommendedMealsForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        Assert.That(config.MealPreferences[0].TagIds, Is.Empty);
    }
}
