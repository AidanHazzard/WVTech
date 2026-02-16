using MealPlanner.Models;

namespace MealPlanner.Helpers;

public static class MealSchedule
{
    public static DateTime BuildStartTime(DateTime date, TimeSpan time)
        => date.Date.Add(time);

    public static List<Meal> MealsForDate(IEnumerable<Meal> meals, DateTime selectedDate)
    {
        var d = selectedDate.Date;
        var start = d;
        var end = d.AddDays(1);

        var exact = meals
            .Where(m => m.StartTime != null)
            .Where(m => m.StartTime >= start && m.StartTime < end);

        var weekly = meals
            .Where(m => m.StartTime != null)
            .Where(m => m.RepeatRule == "Weekly")
            .Where(m => m.StartTime!.Value.DayOfWeek == d.DayOfWeek);

        return exact
            .Concat(weekly)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .OrderBy(m => m.StartTime)
            .ToList();
    }
}
