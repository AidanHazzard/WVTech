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

        _controller = new MealController(
            _registrationServiceMock.Object,
            _recipeRepoMock.Object,
            _mealRepoMock.Object,
            _context,
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
    public void MealSize_SmallSnack_HasExpectedCalories()
    {
        Assert.That(MealSize.SmallSnack.Calories(), Is.EqualTo(150));
    }

    [Test]
    public void MealSize_Small_HasExpectedCalories()
    {
        Assert.That(MealSize.Small.Calories(), Is.EqualTo(400));
    }

    [Test]
    public void MealSize_Average_HasExpectedCalories()
    {
        Assert.That(MealSize.Average.Calories(), Is.EqualTo(600));
    }

    [Test]
    public void MealSize_Large_HasExpectedCalories()
    {
        Assert.That(MealSize.Large.Calories(), Is.EqualTo(800));
    }

    [Test]
    public void MealSize_LargeSnack_HasExpectedCalories()
    {
        Assert.That(MealSize.LargeSnack.Calories(), Is.EqualTo(350));
    }

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
    public void GenerateDayPlan_Get_ReturnsViewResult()
    {
        var result = _controller.GenerateDayPlan();

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public void GenerateDayPlan_Get_ModelHasCurrentMonthAndDay()
    {
        var result = (ViewResult)_controller.GenerateDayPlan();
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
}
