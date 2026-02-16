using MealPlanner.Helpers;
using MealPlanner.Models;
using Xunit;

public class MealScheduleTests
{
    [Fact]
    public void BuildStartTime_CombinesDateAndTime()
    {
        var date = new DateTime(2026, 2, 15);
        var time = new TimeSpan(12, 0, 0);

        var result = MealSchedule.BuildStartTime(date, time);

        Assert.Equal(new DateTime(2026, 2, 15, 12, 0, 0), result);
    }

    [Fact]
    public void MealsForDate_ReturnsExactDateMeals()
    {
        var selected = new DateTime(2026, 2, 15);

        var meals = new List<Meal>
        {
            new Meal { Id = 1, StartTime = new DateTime(2026, 2, 15, 8, 0, 0), RepeatRule = null },
            new Meal { Id = 2, StartTime = new DateTime(2026, 2, 16, 8, 0, 0), RepeatRule = null }
        };

        var result = MealSchedule.MealsForDate(meals, selected);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void MealsForDate_IncludesWeeklyRepeatsOnMatchingWeekday()
    {
        var selected = new DateTime(2026, 2, 15); // Sunday
        var nextSunday = new DateTime(2026, 2, 22); // Sunday

        var meals = new List<Meal>
        {
            new Meal { Id = 10, StartTime = selected.AddHours(9), RepeatRule = "Weekly" }
        };

        var result = MealSchedule.MealsForDate(meals, nextSunday);

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
    }

    [Fact]
    public void MealsForDate_ExcludesWeeklyRepeatsOnNonMatchingWeekday()
    {
        var selected = new DateTime(2026, 2, 15); // Sunday
        var monday = new DateTime(2026, 2, 16); // Monday

        var meals = new List<Meal>
        {
            new Meal { Id = 10, StartTime = selected.AddHours(9), RepeatRule = "Weekly" }
        };

        var result = MealSchedule.MealsForDate(meals, monday);

        Assert.Empty(result);
    }

    [Fact]
    public void MealsForDate_DeduplicatesIfMealMatchesExactAndWeekly()
    {
        var selected = new DateTime(2026, 2, 15);

        var sameMeal = new Meal
        {
            Id = 99,
            StartTime = new DateTime(2026, 2, 15, 12, 0, 0),
            RepeatRule = "Weekly"
        };

        var meals = new List<Meal> { sameMeal };

        var result = MealSchedule.MealsForDate(meals, selected);

        Assert.Single(result);
        Assert.Equal(99, result[0].Id);
    }

    [Fact]
    public void MealsForDate_SortsByStartTime()
    {
        var selected = new DateTime(2026, 2, 15);

        var meals = new List<Meal>
        {
            new Meal { Id = 1, StartTime = new DateTime(2026, 2, 15, 12, 0, 0) },
            new Meal { Id = 2, StartTime = new DateTime(2026, 2, 15, 8, 0, 0) }
        };

        var result = MealSchedule.MealsForDate(meals, selected);

        Assert.Equal(new[] { 2, 1 }, result.Select(m => m.Id).ToArray());
    }
}
