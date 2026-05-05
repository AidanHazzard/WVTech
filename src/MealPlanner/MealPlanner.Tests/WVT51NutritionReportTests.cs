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
using System.Security.Claims;

namespace MealPlanner.Tests;

[TestFixture]
public class WVT51NutritionReportTests
{
    private FoodEntriesController _controller;
    private Mock<INutritionProgressService> _nutritionServiceMock;

    private static readonly NutritionProgressDto _weeklyDto = new(
        UserId: "user-1",
        StartDay: DateOnly.FromDateTime(DateTime.Today.AddDays(-6)),
        EndDay: DateOnly.FromDateTime(DateTime.Today),
        Targets: new MacroTargets(2000, 50, 250, 70),
        Totals: new MacroTotals(1400, 35, 180, 50)
    );

    private static readonly NutritionProgressDto _monthlyDto = new(
        UserId: "user-1",
        StartDay: DateOnly.FromDateTime(DateTime.Today.AddDays(-29)),
        EndDay: DateOnly.FromDateTime(DateTime.Today),
        Targets: new MacroTargets(2000, 50, 250, 70),
        Totals: new MacroTotals(42000, 1050, 5400, 1500)
    );

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection)
            .Options;

        using var seedContext = new MealPlannerDBContext(contextOptions);
        seedContext.Database.EnsureCreated();

        _nutritionServiceMock = new Mock<INutritionProgressService>();

        _nutritionServiceMock
            .Setup(s => s.GetRangeProgressAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(_weeklyDto);

        var recipeRepo = new Mock<IRecipeRepository>();
        var tagRepo = new Mock<ITagRepository>();
        var userRecipeRepo = new Mock<IUserRecipeRepository>();
        var registrationService = new Mock<IRegistrationService>();
        var controllerContext = new MealPlannerDBContext(contextOptions);

        _controller = new FoodEntriesController(
            recipeRepo.Object,
            tagRepo.Object,
            userRecipeRepo.Object,
            controllerContext,
            registrationService.Object,
            nutritionProgressService: _nutritionServiceMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task NutritionReport_ReturnsView_WhenServiceIsPresent()
    {
        var result = await _controller.NutritionReport();

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task NutritionReport_ActiveTab_IsWeekly_ByDefault()
    {
        var result = (ViewResult)await _controller.NutritionReport();
        var model = (NutritionReportViewModel)result.Model!;

        Assert.That(model.ActiveTab, Is.EqualTo("weekly"));
    }

    [Test]
    public async Task NutritionReport_ActiveTab_IsMonthly_WhenTabParamIsMonthly()
    {
        _nutritionServiceMock
            .Setup(s => s.GetRangeProgressAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(_monthlyDto);

        var result = (ViewResult)await _controller.NutritionReport(tab: "monthly");
        var model = (NutritionReportViewModel)result.Model!;

        Assert.That(model.ActiveTab, Is.EqualTo("monthly"));
    }

    [Test]
    public async Task NutritionReport_CallsServiceWithWeeklyRange_WhenTabIsWeekly()
    {
        await _controller.NutritionReport(tab: "weekly");

        _nutritionServiceMock.Verify(
            s => s.GetRangeProgressAsync(
                "user-1",
                It.Is<DateOnly>(d => d == DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
                It.Is<DateOnly>(d => d == DateOnly.FromDateTime(DateTime.Today))),
            Times.Once);
    }

    [Test]
    public async Task NutritionReport_CallsServiceWithMonthlyRange_WhenTabIsMonthly()
    {
        _nutritionServiceMock
            .Setup(s => s.GetRangeProgressAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(_monthlyDto);

        await _controller.NutritionReport(tab: "monthly");

        _nutritionServiceMock.Verify(
            s => s.GetRangeProgressAsync(
                "user-1",
                It.Is<DateOnly>(d => d == DateOnly.FromDateTime(DateTime.Today.AddDays(-29))),
                It.Is<DateOnly>(d => d == DateOnly.FromDateTime(DateTime.Today))),
            Times.Once);
    }

    [Test]
    public async Task NutritionReport_Returns500_WhenServiceIsNull()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection).Options;
        using var ctx = new MealPlannerDBContext(contextOptions);
        ctx.Database.EnsureCreated();

        var controller = new FoodEntriesController(
            new Mock<IRecipeRepository>().Object,
            new Mock<ITagRepository>().Object,
            new Mock<IUserRecipeRepository>().Object,
            ctx,
            new Mock<IRegistrationService>().Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }, "TestAuth"))
            }
        };

        var result = await controller.NutritionReport();

        Assert.That(result, Is.TypeOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(500));
    }
}
