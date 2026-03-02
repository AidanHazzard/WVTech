using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MealPlanner.Controllers;
using MealPlanner.Models;
using MealPlanner.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class FavoritesControllerTests
{
    private static ClaimsPrincipal AuthenticatedPrincipal(string userId = "user-1")
        => new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser")
            },
            authenticationType: "TestAuth"));

    private static UserManager<User> BuildUserManagerMock(out Mock<UserManager<User>> userManagerMock)
    {
        var store = new Mock<IUserStore<User>>();
        userManagerMock = new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        return userManagerMock.Object;
    }

    private static FavoritesController CreateController(
        Mock<IFavoritesService> favoritesServiceMock,
        Mock<UserManager<User>> userManagerMock,
        ClaimsPrincipal? user = null,
        bool isLocalUrl = true)
    {
        var controller = new FavoritesController(favoritesServiceMock.Object, userManagerMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user ?? AuthenticatedPrincipal() }
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(isLocalUrl);
        controller.Url = urlHelper.Object;

        return controller;
    }

    [Test]
    public void Index_RedirectsToMyFavorites()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var controller = CreateController(favSvc, userManagerMock);

        var result = controller.Index();

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
    }

    [Test]
    public async Task Add_WhenUserNull_ReturnsUnauthorized()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(favSvc, userManagerMock);

        var result = await controller.Add(recipeId: 10, returnUrl: "/FoodEntries/Recipes/10");

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        favSvc.Verify(s => s.AddFavoriteAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Add_CallsService_AndRedirectsToLocalReturnUrl_WhenProvided()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: true);

        var returnUrl = "/FoodEntries/Recipes/10";
        var result = await controller.Add(recipeId: 10, returnUrl: returnUrl);

        favSvc.Verify(s => s.AddFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.Url, Is.EqualTo(returnUrl));
    }

    [Test]
    public async Task Add_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlMissing()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: true);

        var result = await controller.Add(recipeId: 10, returnUrl: null);

        favSvc.Verify(s => s.AddFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
    }

    [Test]
    public async Task Add_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlNotLocal()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: false);

        var result = await controller.Add(recipeId: 10, returnUrl: "https://evil.example.com");

        favSvc.Verify(s => s.AddFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
    }

    [Test]
    public async Task Remove_WhenUserNull_ReturnsUnauthorized()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(favSvc, userManagerMock);

        var result = await controller.Remove(recipeId: 10, returnUrl: "/Favorites/MyFavorites");

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        favSvc.Verify(s => s.RemoveFavoriteAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Remove_CallsService_AndRedirectsToLocalReturnUrl_WhenProvided()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: true);

        var returnUrl = "/Favorites/MyFavorites";
        var result = await controller.Remove(recipeId: 10, returnUrl: returnUrl);

        favSvc.Verify(s => s.RemoveFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.Url, Is.EqualTo(returnUrl));
    }

    [Test]
    public async Task Remove_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlMissing()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: true);

        var result = await controller.Remove(recipeId: 10, returnUrl: null);

        favSvc.Verify(s => s.RemoveFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
    }

    [Test]
    public async Task Remove_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlNotLocal()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = CreateController(favSvc, userManagerMock, isLocalUrl: false);

        var result = await controller.Remove(recipeId: 10, returnUrl: "https://evil.example.com");

        favSvc.Verify(s => s.RemoveFavoriteAsync("user-1", 10), Times.Once);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
    }

    [Test]
    public async Task MyFavorites_WhenUserNull_ReturnsUnauthorized()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(favSvc, userManagerMock);

        var result = await controller.MyFavorites();

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        favSvc.Verify(s => s.GetFavoritesAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task MyFavorites_WhenUserExists_ReturnsView_WithFavoritesModel()
    {
        var favSvc = new Mock<IFavoritesService>();
        BuildUserManagerMock(out var userManagerMock);

        var user = new User { Id = "user-1", FullName = "Test User" };
        var favorites = new List<Recipe>
        {
            new Recipe { Id = 1, Name = "Recipe 1", Directions = "D1" },
            new Recipe { Id = 2, Name = "Recipe 2", Directions = "D2" }
        };

        userManagerMock
            .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        favSvc
            .Setup(s => s.GetFavoritesAsync("user-1"))
            .ReturnsAsync(favorites);

        var controller = CreateController(favSvc, userManagerMock);

        var result = await controller.MyFavorites();

        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null);
        Assert.That(view!.Model, Is.TypeOf<List<Recipe>>());

        var model = (List<Recipe>)view.Model!;
        Assert.That(model.Count, Is.EqualTo(2));
        Assert.That(model[0].Name, Is.EqualTo("Recipe 1"));

        favSvc.Verify(s => s.GetFavoritesAsync("user-1"), Times.Once);
    }
}