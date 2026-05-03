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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class ShoppingControllerTests
{
    private ShoppingController _controller;
    private MealPlannerDBContext _context;
    private ClaimsPrincipal _claimsPrincipal;
    private Mock<IRegistrationService> _registrationServiceMock;
    private Mock<IPantryRepository> _pantryRepoMock;
    private Mock<IIngredientBaseRepository> _ingredientBaseRepoMock;
    private Mock<IRepository<Measurement>> _measurementRepoMock;
    private User _user;

    [SetUp]
    public void SetUp()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<MealPlannerDBContext>()
            .UseSqlite(connection)
            .Options;

        _context = new MealPlannerDBContext(contextOptions);
        _context.Database.EnsureCreated();

        _user = new User { Id = "user-1", FullName = "Gary", UserName = "gary@fakeemail.com" };
        _context.Users.Add(_user);
        _context.SaveChanges();

        _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") },
            "TestAuth"));

        _registrationServiceMock = new Mock<IRegistrationService>();
        _registrationServiceMock
            .Setup(r => r.FindUserByClaimAsync(_claimsPrincipal))
            .ReturnsAsync(_user);

        _pantryRepoMock = new Mock<IPantryRepository>();

        _ingredientBaseRepoMock = new Mock<IIngredientBaseRepository>();
        _ingredientBaseRepoMock
            .Setup(r => r.FindOrCreateByName(It.IsAny<string>()))
            .Returns((string name) => new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(name) });

        _measurementRepoMock = new Mock<IRepository<Measurement>>();
        _measurementRepoMock
            .Setup(r => r.FindOrCreate(It.IsAny<System.Linq.Expressions.Expression<Func<Measurement, bool>>>(), It.IsAny<Func<Measurement>>()))
            .Returns((System.Linq.Expressions.Expression<Func<Measurement, bool>> _, Func<Measurement> factory) => factory());

        var pantryService = new PantryService(_pantryRepoMock.Object, _ingredientBaseRepoMock.Object, _measurementRepoMock.Object);
        var shoppingListService = new ShoppingListService(
            new Mock<IShoppingListRepository>().Object,
            _ingredientBaseRepoMock.Object,
            _measurementRepoMock.Object);

        _controller = new ShoppingController(
            shoppingListService,
            pantryService,
            null!,
            _registrationServiceMock.Object,
            new Mock<IMealRepository>().Object,
            _context);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _claimsPrincipal }
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    // --- Scenario 1: item appears in pantry list after add ---

    [Test]
    public async Task AddPantryItem_WithValidModel_RedirectsToPantry()
    {
        var model = new PantryItemViewModel { Name = "Milk", Amount = 2f, Measurement = "cups" };

        var result = await _controller.AddPantryItem(model);

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Pantry"));
    }

    [Test]
    public async Task AddPantryItem_WithValidModel_AddsPantryItemToUser()
    {
        var model = new PantryItemViewModel { Name = "Milk", Amount = 2f, Measurement = "cups" };

        await _controller.AddPantryItem(model);

        var user = await _context.Users
            .Include(u => u.PantryItems)
            .ThenInclude(i => i.IngredientBase)
            .FirstAsync(u => u.Id == "user-1");

        Assert.That(user.PantryItems, Has.Count.EqualTo(1));
        Assert.That(user.PantryItems[0].DisplayName, Is.EqualTo("Milk"));
        Assert.That(user.PantryItems[0].IngredientBase.Name, Is.EqualTo(IngredientNameNormalizer.NormalizeKey("Milk")));
    }

    [Test]
    public async Task Pantry_WhenUserHasPantryItems_ReturnsThemInModel()
    {
        var ingredient = new Ingredient
        {
            DisplayName = "Eggs",
            IngredientBase = new IngredientBase { Name = "egg" },
            Measurement = new Measurement { Name = "pieces" },
            Amount = 12f
        };
        _pantryRepoMock
            .Setup(r => r.GetByUserId("user-1"))
            .Returns(new List<Ingredient> { ingredient });

        var result = await _controller.Pantry() as ViewResult;

        Assert.That(result, Is.Not.Null);
        var items = result!.Model as List<Ingredient>;
        Assert.That(items, Is.Not.Null);
        Assert.That(items!, Has.Count.EqualTo(1));
        Assert.That(items![0].DisplayName, Is.EqualTo("Eggs"));
    }

    // --- Scenario 4: validation prevents empty name ---

    [Test]
    public async Task AddPantryItem_WithEmptyName_RedirectsToPantry()
    {
        var model = new PantryItemViewModel { Name = "", Amount = 3f, Measurement = "cups" };
        _controller.ModelState.AddModelError("Name", "Required");

        var result = await _controller.AddPantryItem(model);

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Pantry"));
    }

    [Test]
    public async Task AddPantryItem_WithEmptyName_DoesNotAddPantryItem()
    {
        var model = new PantryItemViewModel { Name = "", Amount = 3f, Measurement = "cups" };
        _controller.ModelState.AddModelError("Name", "Required");

        await _controller.AddPantryItem(model);

        var user = await _context.Users
            .Include(u => u.PantryItems)
            .FirstAsync(u => u.Id == "user-1");

        Assert.That(user.PantryItems, Is.Empty);
    }
}
