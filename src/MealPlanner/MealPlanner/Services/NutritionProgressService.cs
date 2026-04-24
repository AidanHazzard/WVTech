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

    public Task<NutritionProgressDto> GetDailyProgressAsync(string userId, DateOnly day)
        => GetRangeProgressAsync(userId, day, day);

    public async Task<NutritionProgressDto> GetRangeProgressAsync(string userId, DateOnly startDay, DateOnly endDay)
    {
        var start = startDay.ToDateTime(TimeOnly.MinValue);
        var endExclusive = endDay.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var targets =
            await _db.UserNutritionPreferences
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new MacroTargets(
                    x.CalorieTarget ?? 0,
                    x.ProteinTarget ?? 0,
                    x.CarbTarget ?? 0,
                    x.FatTarget ?? 0
                ))
                .FirstOrDefaultAsync()
            ?? new MacroTargets(0, 0, 0, 0);

        var completedMeals =
            await _db.MealCompletions
                .AsNoTracking()
                .Where(mc =>
                    mc.CompletionDate >= start &&
                    mc.CompletionDate < endExclusive &&
                    mc.Meal.UserId == userId)
                .Include(mc => mc.Meal)
                    .ThenInclude(m => m.Recipes)
                .Select(mc => mc.Meal)
                .ToListAsync();

        var totals = new MacroTotals(
            Calories: completedMeals.Sum(m => m.Recipes.Sum(r => r.Calories)),
            Protein: completedMeals.Sum(m => m.Recipes.Sum(r => r.Protein)),
            Carbs: completedMeals.Sum(m => m.Recipes.Sum(r => r.Carbs)),
            Fat: completedMeals.Sum(m => m.Recipes.Sum(r => r.Fat))
        );

        return new NutritionProgressDto(
            userId,
            startDay,
            endDay,
            targets,
            totals
        );
    }
}