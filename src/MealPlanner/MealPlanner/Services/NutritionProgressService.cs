using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services;

public class NutritionProgressService : INutritionProgressService
{
    private readonly MealPlannerDBContext _db;

    public NutritionProgressService(MealPlannerDBContext db)
    {
        _db = db;
    }

    public async Task<NutritionProgressDto> GetDailyProgressAsync(string userId, DateOnly day)
        => await GetRangeProgressAsync(userId, day, day);

    public async Task<NutritionProgressDto> GetRangeProgressAsync(string userId, DateOnly startDay, DateOnly endDay)
    {
        var start = startDay.ToDateTime(TimeOnly.MinValue);
        var endExclusive = endDay.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var targets = await _db.UserNutritionPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new MacroTargets(
                x.CalorieTarget ?? 0,
                x.ProteinTarget ?? 0,
                x.CarbTarget ?? 0,
                x.FatTarget ?? 0
            ))
            .FirstOrDefaultAsync() ?? new MacroTargets(0, 0, 0, 0);

        var meals = await _db.Meals
            .AsNoTracking()
            .Where(m => m.UserId == userId &&
                        m.StartTime != null &&
                        m.StartTime >= start &&
                        m.StartTime < endExclusive)
            .Include(m => m.Recipes)
            .ToListAsync();

        var totals = new MacroTotals(
            Calories: meals.Sum(m => m.Recipes.Sum(r => (int?)r.Calories ?? 0)),
            Protein:  meals.Sum(m => m.Recipes.Sum(r => (int?)r.Protein ?? 0)),
            Carbs:    meals.Sum(m => m.Recipes.Sum(r => (int?)r.Carbs ?? 0)),
            Fat:      meals.Sum(m => m.Recipes.Sum(r => (int?)r.Fat ?? 0))
        );

        return new NutritionProgressDto(
            UserId: userId,
            StartDay: startDay,
            EndDay: endDay,
            Targets: targets,
            Totals: totals
        );
    }
}