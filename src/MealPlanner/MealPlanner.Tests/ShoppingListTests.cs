using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

public class ShoppingListServiceTests
{
    private Mock<IShoppingListRepository> _repo;
    private Mock<IIngredientBaseRepository> _ingredientBaseRepo;
    private Mock<IRepository<Measurement>> _measurementRepo;
    private ShoppingListService _service;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IShoppingListRepository>();
        _ingredientBaseRepo = new Mock<IIngredientBaseRepository>();
        _measurementRepo = new Mock<IRepository<Measurement>>();

        _ingredientBaseRepo
            .Setup(r => r.FindOrCreateByName(It.IsAny<string>()))
            .Returns((string name) => new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(name) });

        _measurementRepo
            .Setup(r => r.FindOrCreate(It.IsAny<Expression<Func<Measurement, bool>>>(), It.IsAny<Func<Measurement>>()))
            .Returns((Expression<Func<Measurement, bool>> _, Func<Measurement> factory) => factory());

        _service = new ShoppingListService(_repo.Object, _ingredientBaseRepo.Object, _measurementRepo.Object);
    }

    [Test]
    public void AddItem_ShouldAddShoppingListItem_WhenNameIsValid()
    {
        _service.AddItem("user-1", "Milk", 1, "");

        _repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.UserId == "user-1" &&
            i.IngredientBase.Name == IngredientNameNormalizer.NormalizeKey("Milk")
        )), Times.Once);
    }

    [Test]
    public void AddItem_ShouldNormalizeIngredientBaseName_WhenNameHasWhitespaceAndIsPlural()
    {
        _service.AddItem("user-1", "  Eggs  ", 1, "");

        _repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.IngredientBase.Name == IngredientNameNormalizer.NormalizeKey("Eggs")
        )), Times.Once);
    }

    [Test]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _service.AddItem("user-1", "", 1, ""));
        _repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Test]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => _service.AddItem("user-1", "   ", 1, ""));
        _repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Test]
    public void RemoveItem_ShouldCallRepositoryRemove_WhenItemExists()
    {
        _service.RemoveItem(5, "user-1");

        _repo.Verify(r => r.Remove(5, "user-1"), Times.Once);
    }

    [Test]
    public void RemoveItem_ShouldThrowArgumentException_WhenItemIdIsInvalid()
    {
        Assert.Throws<ArgumentException>(() => _service.RemoveItem(0, "user-1"));
        _repo.Verify(r => r.Remove(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void UpdateItemAmount_ShouldCallRepositoryUpdate_WhenValid()
    {
        _service.UpdateItemAmount("user-1", 42, 3.5f);

        _repo.Verify(r => r.UpdateAmountByIngredientBase("user-1", 42, 3.5f), Times.Once);
    }

    [Test]
    public void UpdateItemAmount_ShouldThrowArgumentException_WhenAmountIsNegative()
    {
        // Confirm the same id/user succeeds with a valid amount, proving the negative case is not a false positive
        _service.UpdateItemAmount("user-1", 42, 1f);
        _repo.Verify(r => r.UpdateAmountByIngredientBase("user-1", 42, 1f), Times.Once);
        _repo.Invocations.Clear();

        Assert.Throws<ArgumentException>(() => _service.UpdateItemAmount("user-1", 42, -1f));
        _repo.Verify(r => r.UpdateAmountByIngredientBase(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Test]
    public void GetItemsForUser_ShouldReturnOnlyThatUsersItems()
    {
        var ingredientBase = new IngredientBase { Id = 1, Name = "milk" };
        var measurement = new Measurement { Id = 1, Name = "Cup(s)" };
        var expectedItems = new List<ShoppingListItem>
        {
            new ShoppingListItem { Id = 1, UserId = "user-1", IngredientBase = ingredientBase, Measurement = measurement },
            new ShoppingListItem { Id = 2, UserId = "user-1", IngredientBase = new IngredientBase { Id = 2, Name = "egg" }, Measurement = measurement }
        };

        _repo.Setup(r => r.GetByUserId("user-1")).Returns(expectedItems);

        var result = _service.GetItemsForUser("user-1");

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(item => item.UserId == "user-1"), Is.True);
    }
}
