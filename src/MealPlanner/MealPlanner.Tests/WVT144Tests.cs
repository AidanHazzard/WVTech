using System.Security.Claims;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    // Controller — GenerateDayPlan GET

    [Test]
    public async Task GenerateDayPlan_Get_ReturnsViewResult()
    {
        var result = await _controller.GenerateDayPlan();

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task GenerateDayPlan_Get_ModelHasCurrentMonthAndDay()
    {
        var result = (ViewResult)await _controller.GenerateDayPlan();
        var model = (DayPlanConfigViewModel)result.Model!;

        Assert.That(model.SelectedMonth, Is.EqualTo(DateTime.Today.Month));
        Assert.That(model.SelectedDay, Is.EqualTo(DateTime.Today.Day));
    }

    // Controller — GenerateDayPlan POST

    [Test]
    public async Task GenerateDayPlan_Post_CallsRecommendationServiceWithConfig()
    {
        var config = new DayPlanConfigViewModel
        {
            MealCount = 3,
            IncludeSnacks = false,
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
            .Setup(s => s.GetRecommendedDayPlanForUser(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        _reccServiceMock.Verify(
            s => s.GetRecommendedDayPlanForUser(
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
            .Setup(s => s.GetRecommendedDayPlanForUser(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        var result = await _controller.GenerateDayPlan(config);

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("DayPlanSummary"));
    }

    // Controller — RegenerateMeal GET

    [Test]
    public async Task RegenerateMeal_Get_ReturnsViewWithPrefillledPreferences()
    {
        var meal = new Meal { Id = 5, UserId = "user-1" };
        _mealRepoMock.Setup(r => r.ReadAsync(5)).ReturnsAsync(meal);

        var result = await _controller.RegenerateMeal(5);

        Assert.That(result, Is.TypeOf<ViewResult>());
        var model = ((ViewResult)result).Model;
        Assert.That(model, Is.TypeOf<MealPreferenceViewModel>());
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
            .Setup(s => s.GetRecommendedDayPlanForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
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
            .Setup(s => s.GetRecommendedDayPlanForUser(It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DayPlanConfigViewModel>()))
            .ReturnsAsync([]);

        await _controller.GenerateDayPlan(config);

        Assert.That(config.MealPreferences[0].TagIds, Is.Empty);
    }
}
