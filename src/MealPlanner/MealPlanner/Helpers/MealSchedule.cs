using MealPlanner.Models;

namespace MealPlanner.Helpers;

public static class MealSchedule
{
    public static DateTime BuildStartTime(DateTime date, TimeSpan time)
        => date.Date.Add(time);

    public static IEnumerable<DayOfWeek> ParseRepeatDays(string? repeatDays)
    {
        if (string.IsNullOrWhiteSpace(repeatDays))
            return Enumerable.Empty<DayOfWeek>();

        return repeatDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => (DayOfWeek)int.Parse(s.Trim()));
    }

    public static string? EncodeRepeatDays(IEnumerable<DayOfWeek> days)
    {
        var list = days.ToList();
        return list.Count == 0 ? null : string.Join(",", list.Select(d => (int)d));
    }

    public static bool RepeatMatchesDay(Meal meal, DayOfWeek day)
    {
        if (meal.RepeatRule != "Weekly") return false;

        if (!string.IsNullOrWhiteSpace(meal.RepeatDays))
            return ParseRepeatDays(meal.RepeatDays).Contains(day);

        // Backward compat: no RepeatDays stored, fall back to StartTime.DayOfWeek
        return meal.StartTime?.DayOfWeek == day;
    }

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
            .Where(m => RepeatMatchesDay(m, d.DayOfWeek));

        return exact
            .Concat(weekly)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .OrderBy(m => m.StartTime)
            .ToList();
    }
}
