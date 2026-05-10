using System.Collections.Generic;
using System.Linq.Expressions;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

public class PantryAutoRemoveTests
{
    private Mock<IUserRepository> _userRepo;
    private Mock<IMealAutoRemovedIngredientRepository> _autoRemovedRepo;
    private Mock<IIngredientBaseRepository> _ingredientBaseRepo;
    private Mock<IRepository<Measurement>> _measurementRepo;
    private PantryService _service;

    private static IngredientBase MakeBase(int id, string name) => new IngredientBase { Id = id, Name = name };
    private static Measurement MakeMeasure(int id, string name) => new Measurement { Id = id, Name = name };

    [SetUp]
    public void SetUp()
    {
        _userRepo = new Mock<IUserRepository>();
        _autoRemovedRepo = new Mock<IMealAutoRemovedIngredientRepository>();
        _ingredientBaseRepo = new Mock<IIngredientBaseRepository>();
        _measurementRepo = new Mock<IRepository<Measurement>>();

        _ingredientBaseRepo
            .Setup(r => r.FindOrCreateByName(It.IsAny<string>()))
            .Returns((string name) => new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(name) });

        _measurementRepo
            .Setup(r => r.FindOrCreate(It.IsAny<Expression<Func<Measurement, bool>>>(), It.IsAny<Func<Measurement>>()))
            .Returns((Expression<Func<Measurement, bool>> _, Func<Measurement> factory) => factory());

        _service = new PantryService(_userRepo.Object, _autoRemovedRepo.Object, _ingredientBaseRepo.Object, _measurementRepo.Object);
    }

    private static Meal MakeMeal(int id, List<Ingredient> ingredients)
    {
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Directions = "" };
        recipe.Ingredients.AddRange(ingredients);
        return new Meal { Id = id, Recipes = [recipe] };
    }

    // ── AddPantryItem ─────────────────────────────────────────────────────────

    [Test]
    public void AddPantryItem_MergesAmount_WhenSameIngredientAndMeasurementExists()
    {
        var chicken = MakeBase(10, "chicken");
        var measure = MakeMeasure(5, "Pound(s)");
        var existing = new Ingredient { Id = 1, IngredientBase = chicken, Measurement = measure, Amount = 3 };

        _userRepo.Setup(r => r.GetByUserId("u1")).Returns([existing]);

        var newItem = new Ingredient { IngredientBase = chicken, Measurement = measure, Amount = 2 };
        _service.AddPantryItem("u1", newItem);

        _userRepo.Verify(r => r.UpdateItemAmount(1, "u1", 5), Times.Once);
        _userRepo.Verify(r => r.AddItem(It.IsAny<string>(), It.IsAny<Ingredient>()), Times.Never);
    }

    [Test]
    public void AddPantryItem_AddsNewRow_WhenNoMatchingItemExists()
    {
        var chicken = MakeBase(10, "chicken");
        var measure = MakeMeasure(5, "Pound(s)");

        _userRepo.Setup(r => r.GetByUserId("u1")).Returns([]);

        var newItem = new Ingredient { IngredientBase = chicken, Measurement = measure, Amount = 2 };
        _service.AddPantryItem("u1", newItem);

        _userRepo.Verify(r => r.AddItem("u1", newItem), Times.Once);
        _userRepo.Verify(r => r.UpdateItemAmount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    [Test]
    public void AddPantryItem_AddsNewRow_WhenSameIngredientDifferentMeasurementExists()
    {
        var chicken = MakeBase(10, "chicken");
        var existing = new Ingredient { Id = 1, IngredientBase = chicken, Measurement = MakeMeasure(5, "Pound(s)"), Amount = 3 };

        _userRepo.Setup(r => r.GetByUserId("u1")).Returns([existing]);

        var newItem = new Ingredient { IngredientBase = chicken, Measurement = MakeMeasure(6, "Ounce(s)"), Amount = 8 };
        _service.AddPantryItem("u1", newItem);

        _userRepo.Verify(r => r.AddItem("u1", newItem), Times.Once);
        _userRepo.Verify(r => r.UpdateItemAmount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    // ── GetMealPantryMatchIds ─────────────────────────────────────────────────

    [Test]
    public void GetMealPantryMatchIds_ReturnsMatchingMealIds_WhenOverlapExists()
    {
        var chicken = MakeBase(10, "chicken");
        _userRepo.Setup(r => r.GetUserIngredientBaseIds("u1"))
            .Returns([10]);

        var ingredient = new Ingredient { IngredientBase = chicken, Measurement = MakeMeasure(1, "Pound(s)"), Amount = 2 };
        var meal = MakeMeal(42, [ingredient]);

        var result = _service.GetMealPantryMatchIds("u1", [meal]);

        Assert.That(result, Contains.Item(42));
    }

    [Test]
    public void GetMealPantryMatchIds_ReturnsEmpty_WhenNoPantryItems()
    {
        _userRepo.Setup(r => r.GetUserIngredientBaseIds("u1"))
            .Returns([]);

        var ingredient = new Ingredient { IngredientBase = MakeBase(10, "chicken"), Measurement = MakeMeasure(1, "Pound(s)"), Amount = 2 };
        var meal = MakeMeal(42, [ingredient]);

        var result = _service.GetMealPantryMatchIds("u1", [meal]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetMealPantryMatchIds_ReturnsEmpty_WhenNoIngredientOverlap()
    {
        _userRepo.Setup(r => r.GetUserIngredientBaseIds("u1"))
            .Returns([99]);

        var ingredient = new Ingredient { IngredientBase = MakeBase(10, "chicken"), Measurement = MakeMeasure(1, "Pound(s)"), Amount = 2 };
        var meal = MakeMeal(42, [ingredient]);

        var result = _service.GetMealPantryMatchIds("u1", [meal]);

        Assert.That(result, Is.Empty);
    }

    // ── AutoRemovePantryItems ────────────────────────────────────────────

    [Test]
    public void AutoRemovePantryItems_DeductsRecipeAmount_WhenPantryExceedsRecipeAmount()
    {
        var chicken = MakeBase(10, "chicken");
        var measure = MakeMeasure(5, "Pound(s)");
        var pantryItem = new Ingredient { Id = 1, DisplayName = "Chicken", IngredientBase = chicken, Measurement = measure, Amount = 2 };

        var meal = MakeMeal(42, [new Ingredient { IngredientBase = chicken, Measurement = measure, Amount = 1 }]);

        _userRepo.Setup(r => r.GetMatchingPantryItems("u1", It.IsAny<HashSet<int>>()))
            .Returns([pantryItem]);

        List<MealAutoRemovedIngredient>? capturedRecords = null;
        _autoRemovedRepo.Setup(r => r.AddRange(It.IsAny<List<MealAutoRemovedIngredient>>()))
            .Callback<List<MealAutoRemovedIngredient>>(r => capturedRecords = r);

        _service.AutoRemovePantryItems("u1", 42, DateTime.Today, [meal]);

        Assert.That(capturedRecords, Is.Not.Null);
        Assert.That(capturedRecords!.Count, Is.EqualTo(1));
        Assert.That(capturedRecords[0].IngredientBaseId, Is.EqualTo(10));
        Assert.That(capturedRecords[0].Amount, Is.EqualTo(1), "Should record only the recipe amount consumed");
        _userRepo.Verify(r => r.UpdateItemAmount(1, "u1", 1), Times.Once);
        _userRepo.Verify(r => r.RemoveItem(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void AutoRemovePantryItems_RemovesWholeRow_WhenPantryAmountLessOrEqualRecipeAmount()
    {
        var chicken = MakeBase(10, "chicken");
        var measure = MakeMeasure(5, "Pound(s)");
        var pantryItem = new Ingredient { Id = 1, DisplayName = "Chicken", IngredientBase = chicken, Measurement = measure, Amount = 1 };

        var meal = MakeMeal(42, [new Ingredient { IngredientBase = chicken, Measurement = measure, Amount = 2 }]);

        _userRepo.Setup(r => r.GetMatchingPantryItems("u1", It.IsAny<HashSet<int>>()))
            .Returns([pantryItem]);

        List<MealAutoRemovedIngredient>? capturedRecords = null;
        _autoRemovedRepo.Setup(r => r.AddRange(It.IsAny<List<MealAutoRemovedIngredient>>()))
            .Callback<List<MealAutoRemovedIngredient>>(r => capturedRecords = r);

        _service.AutoRemovePantryItems("u1", 42, DateTime.Today, [meal]);

        Assert.That(capturedRecords, Is.Not.Null);
        Assert.That(capturedRecords!.Count, Is.EqualTo(1));
        Assert.That(capturedRecords[0].Amount, Is.EqualTo(1), "Should record full pantry amount");
        _userRepo.Verify(r => r.RemoveItem(1, "u1"), Times.Once);
        _userRepo.Verify(r => r.UpdateItemAmount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    [Test]
    public void AutoRemovePantryItems_DoesNothing_WhenMealNotInList()
    {
        var meal = MakeMeal(99, []);

        _service.AutoRemovePantryItems("u1", 42, DateTime.Today, [meal]);

        _userRepo.Verify(r => r.GetMatchingPantryItems(It.IsAny<string>(), It.IsAny<HashSet<int>>()), Times.Never);
    }

    // ── HasAutoRemovedIngredients ─────────────────────────────────────────────

    [Test]
    public void HasAutoRemovedIngredients_ReturnsTrue_WhenRecordsExist()
    {
        var record = new MealAutoRemovedIngredient
        {
            MealId = 42, CompletionDate = DateTime.Today,
            IngredientBase = MakeBase(10, "chicken"), Measurement = MakeMeasure(5, "Pound(s)")
        };
        _autoRemovedRepo.Setup(r => r.GetByMealAndDate(42, DateTime.Today))
            .Returns([record]);

        Assert.That(_service.HasAutoRemovedIngredients(42, DateTime.Today), Is.True);
    }

    [Test]
    public void HasAutoRemovedIngredients_ReturnsFalse_WhenNoRecords()
    {
        _autoRemovedRepo.Setup(r => r.GetByMealAndDate(42, DateTime.Today))
            .Returns([]);

        Assert.That(_service.HasAutoRemovedIngredients(42, DateTime.Today), Is.False);
    }

    // ── RestorePantryItems ───────────────────────────────────────────────

    [Test]
    public void RestorePantryItems_AddsIngredientBackAndDeletesRecord()
    {
        var record = new MealAutoRemovedIngredient
        {
            MealId = 42, CompletionDate = DateTime.Today,
            IngredientBaseId = 10, IngredientBase = MakeBase(10, "chicken"),
            MeasurementId = 5, Measurement = MakeMeasure(5, "Pound(s)"),
            DisplayName = "Chicken", Amount = 2
        };
        _autoRemovedRepo.Setup(r => r.GetByMealAndDate(42, DateTime.Today))
            .Returns([record]);
        _userRepo.Setup(r => r.GetByUserId("u1")).Returns([]);

        _service.RestorePantryItems("u1", 42, DateTime.Today);

        _userRepo.Verify(r => r.AddItem("u1", It.Is<Ingredient>(i =>
            i.IngredientBase.Id == 10 && i.Amount == 2)), Times.Once);
        _autoRemovedRepo.Verify(r => r.RemoveByMealAndDate(42, DateTime.Today), Times.Once);
    }
}
