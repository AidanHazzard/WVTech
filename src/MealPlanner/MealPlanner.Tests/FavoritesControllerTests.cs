
// using System.Security.Claims;
// using MealPlanner.Controllers;
// using MealPlanner.DAL.Abstract;
// using MealPlanner.Models;
// using MealPlanner.Services;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using Moq;

// namespace MealPlanner.Tests;

// [TestFixture]
// public class FavoritesControllerTests
// {
//     private FavoritesController _controller;
//     private MealPlannerDBContext _context;

//     [SetUp]
//     public void SetUp()
//     {

//         var connection = new SqliteConnection("Filename=:memory:");
//         connection.Open();
//         var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
//             .UseSqlite(connection)
//             .Options;
//         _context = new MealPlannerDBContext(contextOptions);

//         var user = new ClaimsPrincipal(new ClaimsIdentity(
//             [
//                 new Claim(ClaimTypes.NameIdentifier, "user-1"),
//                 new Claim(ClaimTypes.Name, "testuser")
//             ],
//             authenticationType: "TestAuth"));
        
//         var regServiceMock = new Mock<IRegistrationService>();
//         regServiceMock.Setup(r => r.FindUserByClaimAsync(user))
//             .ReturnsAsync(new User { FullName = "testuser", Id = "test"});

//         var userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        
//         List<Recipe> favorites = 
//             [
//                 new Recipe { Id = 1, Name = "Recipe 1", Directions = "D1" },
//                 new Recipe { Id = 2, Name = "Recipe 2", Directions = "D2" }
//             ];
        
//         userRecipeRepoMock.Setup(ur => ur.GetFavoritesAsync("test"))
//             .ReturnsAsync(favorites);

//         _controller = new FavoritesController(
//             _context, 
//             userRecipeRepoMock.Object,
//             regServiceMock.Object,
//             new Mock<IRecipeRepository>().Object);
//         _controller.ControllerContext = new ControllerContext
//         {
//             HttpContext = new DefaultHttpContext { User = user }
//         };
//         _controller.Url = new Mock<IUrlHelper>().Object;
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         _controller.Dispose();
//         _context.Dispose();
//     }

//     [Test]
//     public void Index_RedirectsToMyFavorites()
//     {
//         var result = _controller.Index();

//         Assert.That(result, Is.TypeOf<RedirectToActionResult>());
//         var redirect = result as RedirectToActionResult;

//         Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
//     }

//     [Test]
//     public async Task Add_WhenUserNull_ReturnsUnauthorized()
//     {
//         _controller.ControllerContext.HttpContext.User = null;
//         var result = await _controller.Add(recipeId: 10, returnUrl: "/FoodEntries/Recipes/10");

//         Assert.That(result, Is.TypeOf<UnauthorizedResult>());
//     }

//     [Test]
//     public async Task Add_CallsService_AndRedirectsToLocalReturnUrl_WhenProvided()
//     {
//         var urlMock = new Mock<IUrlHelper>();
//         urlMock.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
//         _controller.Url = urlMock.Object;
        
//         var returnUrl = "/FoodEntries/Recipes/10";
//         var result = await _controller.Add(recipeId: 10, returnUrl: returnUrl);

//         Assert.That(result, Is.TypeOf<RedirectResult>());
//         var redirect = result as RedirectResult;
//         Assert.That(redirect!.Url, Is.EqualTo(returnUrl));
//     }

//     [Test]
//     public async Task Add_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlMissing()
//     {
//         var result = await _controller.Add(recipeId: 10, returnUrl: null);
        
//         Assert.That(result, Is.TypeOf<RedirectToActionResult>());

//         var redirect = result as RedirectToActionResult;
//         Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
//     }

//     [Test]
//     public async Task Add_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlNotLocal()
//     {
//         var result = await _controller.Add(recipeId: 10, returnUrl: "https://evil.example.com");

//         Assert.That(result, Is.TypeOf<RedirectToActionResult>());
//         var redirect = result as RedirectToActionResult;

//         Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
//     }

//     [Test]
//     public async Task Remove_WhenUserNull_ReturnsUnauthorized()
//     {
//         _controller.ControllerContext = new ControllerContext();
//         var result = await _controller.Remove(recipeId: 10, returnUrl: "/Favorites/MyFavorites");

//         Assert.That(result, Is.TypeOf<UnauthorizedResult>());
//     }

//     [Test]
//     public async Task Remove_CallsService_AndRedirectsToLocalReturnUrl_WhenProvided()
//     {
//         var urlMock = new Mock<IUrlHelper>();
//         urlMock.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
//         _controller.Url = urlMock.Object;

//         var returnUrl = "/Favorites/MyFavorites";
//         var result = await _controller.Remove(recipeId: 10, returnUrl: returnUrl);

//         Assert.That(result, Is.TypeOf<RedirectResult>());
//         var redirect = result as RedirectResult;

//         Assert.That(redirect!.Url, Is.EqualTo(returnUrl));
//     }

//     [Test]
//     public async Task Remove_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlMissing()
//     {
//         var result = await _controller.Remove(recipeId: 10, returnUrl: null);
        
//         var redirect = result as RedirectToActionResult;
//         Assert.That(redirect, Is.Not.Null);
//         Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
//     }

//     [Test]
//     public async Task Remove_CallsService_AndRedirectsToMyFavorites_WhenReturnUrlNotLocal()
//     {
//         var result = await _controller.Remove(recipeId: 10, returnUrl: "https://evil.example.com");

//         Assert.That(result, Is.TypeOf<RedirectToActionResult>());
//         var redirect = result as RedirectToActionResult;

//         Assert.That(redirect!.ActionName, Is.EqualTo("MyFavorites"));
//     }

//     [Test]
//     public async Task MyFavorites_WhenUserNull_ReturnsUnauthorized()
//     {
//         _controller.ControllerContext = new ControllerContext();
//         var result = await _controller.MyFavorites();

//         Assert.That(result, Is.TypeOf<UnauthorizedResult>());
//     }

//     [Test]
//     public async Task MyFavorites_WhenUserExists_ReturnsView_WithFavoritesModel()
//     {

//         var result = await _controller.MyFavorites();
        
//         Assert.That(result, Is.TypeOf<ViewResult>());
//         var view = result as ViewResult;
//         Assert.That(view!.Model, Is.TypeOf<List<Recipe>>());

//         var model = (List<Recipe>)view.Model!;
//         Assert.That(model.Count, Is.EqualTo(2));
//         Assert.That(model[0].Name, Is.EqualTo("Recipe 1"));
//     }
// }
