using System;
using System.Collections.Generic;
using System.Linq;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using Moq;
using Xunit;

namespace MealPlanner.Tests;

public class MealPlannerServiceTests
{
    [Fact]
    public void RemoveMealOccurrence_ShouldRemoveOnlySelectedMealOccurrence_WhenMealExists()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealPlannerService(repo.Object);

        int mealId = 10;
        int userId = 1;
        DateOnly mealDate = new DateOnly(2026, 3, 10);

        service.RemoveMealOccurrence(mealId, userId, mealDate);

        repo.Verify(r => r.RemoveMealOccurrence(mealId, userId, mealDate), Times.Once);
    }

    [Fact]
    public void RemoveMealOccurrence_ShouldThrowArgumentException_WhenMealIdIsInvalid()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealPlannerService(repo.Object);

        var mealDate = new DateOnly(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => service.RemoveMealOccurrence(0, 1, mealDate));
        repo.Verify(r => r.RemoveMealOccurrence(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void RemoveMealOccurrence_ShouldThrowArgumentException_WhenUserIdIsInvalid()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealPlannerService(repo.Object);

        var mealDate = new DateOnly(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => service.RemoveMealOccurrence(10, 0, mealDate));
        repo.Verify(r => r.RemoveMealOccurrence(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void RemoveMealOccurrence_ShouldNotRemoveFutureRepeatedMeals_WhenMealHasRepeatRule()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealPlannerService(repo.Object);

        int mealId = 10;
        int userId = 1;
        DateOnly selectedDate = new DateOnly(2026, 3, 10);

        service.RemoveMealOccurrence(mealId, userId, selectedDate);

        repo.Verify(r => r.RemoveMealOccurrence(mealId, userId, selectedDate), Times.Once);
        repo.Verify(r => r.RemoveFutureRepeatedMeals(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void GetMealsForDate_ShouldReturnMealsScheduledForThatDate()
    {
        var repo = new Mock<IMealRepository>();
        var targetDate = new DateOnly(2026, 3, 10);

        var meals = new List<Meal>
        {
            new Meal { Id = 1, UserId = 1, Title = "Breakfast", Date = targetDate },
            new Meal { Id = 2, UserId = 1, Title = "Dinner", Date = targetDate }
        };

        repo.Setup(r => r.GetMealsForUserByDate(1, targetDate)).Returns(meals);

        var service = new MealPlannerService(repo.Object);

        var result = service.GetMealsForDate(1, targetDate);

        Assert.Equal(2, result.Count());
        Assert.All(result, meal => Assert.Equal(targetDate, meal.Date));
    }

    [Fact]
    public void RemoveMealOccurrence_ShouldCallRepositoryOnce_WhenConfirmed()
    {
        var repo = new Mock<IMealRepository>();
        var service = new MealPlannerService(repo.Object);

        int mealId = 25;
        int userId = 2;
        DateOnly targetDate = new DateOnly(2026, 3, 12);

        service.RemoveMealOccurrence(mealId, userId, targetDate);

        repo.Verify(r => r.RemoveMealOccurrence(mealId, userId, targetDate), Times.Once);
    }
}