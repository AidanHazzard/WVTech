using System;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using Xunit;

namespace MealPlanner.Tests;

public class MealCreationServiceTests
{
    [Fact]
    public void CreateMeal_ShouldCreateMeal_WhenTitleAndDateAreValid()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        int userId = 1;
        string title = "Breakfast";
        DateOnly mealDate = new DateOnly(2026, 3, 10);

        service.CreateMeal(userId, title, mealDate);

        repo.Verify(r => r.Add(It.Is<Meal>(m =>
            m.UserId == userId &&
            m.Title == "Breakfast" &&
            m.Date == mealDate
        )), Times.Once);
    }

    [Fact]
    public void CreateMeal_ShouldTrimTitle_WhenTitleHasExtraSpaces()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        int userId = 1;
        string title = "  Lunch  ";
        DateOnly mealDate = new DateOnly(2026, 3, 10);

        service.CreateMeal(userId, title, mealDate);

        repo.Verify(r => r.Add(It.Is<Meal>(m =>
            m.UserId == userId &&
            m.Title == "Lunch" &&
            m.Date == mealDate
        )), Times.Once);
    }

    [Fact]
    public void CreateMeal_ShouldThrowArgumentException_WhenTitleIsEmpty()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        var mealDate = new DateOnly(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => service.CreateMeal(1, "", mealDate));
        repo.Verify(r => r.Add(It.IsAny<Meal>()), Times.Never);
    }

    [Fact]
    public void CreateMeal_ShouldThrowArgumentException_WhenTitleIsWhitespace()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        var mealDate = new DateOnly(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => service.CreateMeal(1, "   ", mealDate));
        repo.Verify(r => r.Add(It.IsAny<Meal>()), Times.Never);
    }

    [Fact]
    public void CreateMeal_ShouldThrowArgumentException_WhenUserIdIsInvalid()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        var mealDate = new DateOnly(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => service.CreateMeal(0, "Dinner", mealDate));
        repo.Verify(r => r.Add(It.IsAny<Meal>()), Times.Never);
    }

    [Fact]
    public void CreateMeal_ShouldStoreOnlyDateAndTitle_WhenMealIsCreated()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealCreationService(repo.Object);

        int userId = 1;
        string title = "Snack";
        DateOnly mealDate = new DateOnly(2026, 3, 10);

        service.CreateMeal(userId, title, mealDate);

        repo.Verify(r => r.Add(It.Is<Meal>(m =>
            m.UserId == userId &&
            m.Title == "Snack" &&
            m.Date == mealDate
        )), Times.Once);
    }

    [Fact]
    public void CreateMeal_ShouldDisplayMealByTitle_WhenReturnedToPlanner()
    {
        var repo = new Mock<IMealRepository>();
        var createdMeal = new Meal
        {
            Id = 5,
            UserId = 1,
            Title = "Dinner",
            Date = new DateOnly(2026, 3, 10)
        };

        repo.Setup(r => r.GetById(5)).Returns(createdMeal);

        var service = new MealCreationService(repo.Object);

        var result = service.GetMealById(5);

        Assert.NotNull(result);
        Assert.Equal("Dinner", result.Title);
    }
}