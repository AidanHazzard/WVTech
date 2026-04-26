using System;
using System.Collections.Generic;
using System.Linq;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

public class ShoppingListServiceTests
{
    [Test]
    public void AddItem_ShouldAddShoppingListItem_WhenNameIsValid()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        string userId = "user-1";
        string itemName = "Milk";

        service.AddItem(userId, itemName, 1, "");

        repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.UserId == userId &&
            i.Name == "Milk"
        )), Times.Once);
    }

    [Test]
    public void AddItem_ShouldTrimName_WhenNameHasExtraSpaces()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        string userId = "user-1";
        string itemName = "  Eggs  ";

        service.AddItem(userId, itemName, 1, "");

        repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.UserId == userId &&
            i.Name == "Eggs"
        )), Times.Once);
    }

    [Test]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.AddItem("user-1", "", 1, ""));
        repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Test]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsWhitespace()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.AddItem("user-1", "   ", 1, ""));
        repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Test]
    public void RemoveItem_ShouldCallRepositoryRemove_WhenItemExists()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        int itemId = 5;
        string userId = "user-1";

        service.RemoveItem(itemId, userId);

        repo.Verify(r => r.Remove(itemId, userId), Times.Once);
    }

    [Test]
    public void RemoveItem_ShouldThrowArgumentException_WhenItemIdIsInvalid()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.RemoveItem(0, "user-1"));
        repo.Verify(r => r.Remove(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void UpdateItemAmount_ShouldCallRepositoryUpdateAmountByName_WhenValid()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        service.UpdateItemAmount("user-1", "Milk", 3.5f);

        repo.Verify(r => r.UpdateAmountByName("user-1", "Milk", 3.5f), Times.Once);
    }

    [Test]
    public void UpdateItemAmount_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.UpdateItemAmount("user-1", "", 1f));
        repo.Verify(r => r.UpdateAmountByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    [Test]
    public void UpdateItemAmount_ShouldThrowArgumentException_WhenAmountIsNegative()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.UpdateItemAmount("user-1", "Milk", -1f));
        repo.Verify(r => r.UpdateAmountByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    [Test]
    public void GetItemsForUser_ShouldReturnOnlyThatUsersItems()
    {
        var repo = new Mock<IShoppingListRepository>();
        var expectedItems = new List<ShoppingListItem>
        {
            new ShoppingListItem { Id = 1, UserId = "user-1", Name = "Milk" },
            new ShoppingListItem { Id = 2, UserId = "user-1", Name = "Eggs" }
        };

        repo.Setup(r => r.GetByUserId("user-1")).Returns(expectedItems);
        var service = new ShoppingListService(repo.Object);

        var result = service.GetItemsForUser("user-1");

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(item => item.UserId == "user-1"), Is.True);
    }
}