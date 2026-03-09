using System;
using System.Collections.Generic;
using System.Linq;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using Xunit;

namespace MealPlanner.Tests;

public class ShoppingListServiceTests
{
    [Fact]
    public void AddItem_ShouldAddShoppingListItem_WhenNameIsValid()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        int userId = 1;
        string itemName = "Milk";

        service.AddItem(userId, itemName);

        repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.UserId == userId &&
            i.Name == "Milk"
        )), Times.Once);
    }

    [Fact]
    public void AddItem_ShouldTrimName_WhenNameHasExtraSpaces()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        int userId = 1;
        string itemName = "  Eggs  ";

        service.AddItem(userId, itemName);

        repo.Verify(r => r.Add(It.Is<ShoppingListItem>(i =>
            i.UserId == userId &&
            i.Name == "Eggs"
        )), Times.Once);
    }

    [Fact]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.AddItem(1, ""));
        repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Fact]
    public void AddItem_ShouldThrowArgumentException_WhenNameIsWhitespace()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.AddItem(1, "   "));
        repo.Verify(r => r.Add(It.IsAny<ShoppingListItem>()), Times.Never);
    }

    [Fact]
    public void RemoveItem_ShouldCallRepositoryRemove_WhenItemExists()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        int itemId = 5;
        int userId = 1;

        service.RemoveItem(itemId, userId);

        repo.Verify(r => r.Remove(itemId, userId), Times.Once);
    }

    [Fact]
    public void RemoveItem_ShouldThrowArgumentException_WhenItemIdIsInvalid()
    {
        var repo = new Mock<IShoppingListRepository>();
        var service = new ShoppingListService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.RemoveItem(0, 1));
        repo.Verify(r => r.Remove(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetItemsForUser_ShouldReturnOnlyThatUsersItems()
    {
        var repo = new Mock<IShoppingListRepository>();
        var expectedItems = new List<ShoppingListItem>
        {
            new ShoppingListItem { Id = 1, UserId = 1, Name = "Milk" },
            new ShoppingListItem { Id = 2, UserId = 1, Name = "Eggs" }
        };

        repo.Setup(r => r.GetByUserId(1)).Returns(expectedItems);
        var service = new ShoppingListService(repo.Object);

        var result = service.GetItemsForUser(1);

        Assert.Equal(2, result.Count());
        Assert.All(result, item => Assert.Equal(1, item.UserId));
    }
}