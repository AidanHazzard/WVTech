using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class HomeControllerTests
{
    private static ClaimsPrincipal UnauthenticatedUser()
        => new ClaimsPrincipal(new ClaimsIdentity());

    private static ClaimsPrincipal AuthenticatedUser(string userId = "user-1")
        => new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser")
            },
            authenticationType: "TestAuth"));

    private static HomeController CreateController(
        ClaimsPrincipal user,
        Mock<IRegistrationService>? registrationServiceMock = null,
        Mock<IMealRepository>? mealRepoMock = null)
    {
        registrationServiceMock ??= new Mock<IRegistrationService>();
        mealRepoMock ??= new Mock<IMealRepository>();

        var controller = new HomeController(
            null!,                         // MealPlannerDBContext not used in tested actions
            Mock.Of<ILoginService>(),       // also not used here
            registrationServiceMock.Object,
            mealRepoMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Test]
    public void Landing_WhenUnauthenticated_RedirectsToLoginControllerLogin()
    {
        var controller = CreateController(UnauthenticatedUser());

        var result = controller.Landing();

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ControllerName, Is.EqualTo("Login"));
        Assert.That(redirect.ActionName, Is.EqualTo("Login"));
    }

    [Test]
    public void Landing_WhenAuthenticated_RedirectsToIndex()
    {
        var controller = CreateController(AuthenticatedUser());

        var result = controller.Landing();

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ControllerName, Is.Null);
        Assert.That(redirect.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task Index_WhenUnauthenticated_RedirectsToLoginPath()
    {
        var controller = CreateController(UnauthenticatedUser());

        var result = await controller.Index(date: null);

        var redirect = result as RedirectResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.Url, Is.EqualTo("/Login"));
    }

    [Test]
    public async Task Index_WhenAuthenticated_ReturnsView_WithPlannerHomeViewModel()
    {
        var user = new User { Id = "user-1", FullName = "Test User" };

        var reg = new Mock<IRegistrationService>();
        reg.Setup(r => r.FindUserByClaimAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mealRepo = new Mock<IMealRepository>();
        mealRepo.Setup(m => m.GetUserMealsByDateAsync(It.IsAny<User>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Meal>());

        var controller = CreateController(AuthenticatedUser(), reg, mealRepo);

        var result = await controller.Index(date: null);

        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null);
        Assert.That(view!.Model, Is.TypeOf<PlannerHomeViewModel>());

        var vm = (PlannerHomeViewModel)view.Model!;
        Assert.That(vm.Meals, Is.Not.Null);
    }

    [Test]
    public async Task Index_WhenAuthenticated_ParsesDate_AndPassesParsedDateToRepo()
    {
        var user = new User { Id = "user-1", FullName = "Test User" };

        var reg = new Mock<IRegistrationService>();
        reg.Setup(r => r.FindUserByClaimAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mealRepo = new Mock<IMealRepository>();
        mealRepo.Setup(m => m.GetUserMealsByDateAsync(It.IsAny<User>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Meal>());

        var controller = CreateController(AuthenticatedUser(), reg, mealRepo);

        var input = "2026-02-28";
        var expected = new DateTime(2026, 2, 28);

        var result = await controller.Index(date: input);

        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null);

        var vm = view!.Model as PlannerHomeViewModel;
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.SelectedDate, Is.EqualTo(expected));

        mealRepo.Verify(m => m.GetUserMealsByDateAsync(
            It.Is<User>(u => u.Id == "user-1"),
            It.Is<DateTime>(d => d == expected)), Times.Once);
    }
}