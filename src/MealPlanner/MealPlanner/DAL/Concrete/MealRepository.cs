using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class MealRepository : Repository<Meal>, IMealRepository
{
    public MealRepository(MealPlannerDBContext context) : base(context)
    {
        
    }

    public async Task<List<Meal>> GetUserMealsByDateAsync(User user, DateTime date)
    {
        var start = date;
        var end = date.AddDays(1);

        var exactDateMeals = await _dbset
            .Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.StartTime != null)
            .Where(m => m.StartTime >= start && m.StartTime < end)
            .ToListAsync();

        var weeklyRepeatMeals = await _dbset
            .Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.StartTime != null)
            .Where(m => m.RepeatRule == "Weekly")
            .ToListAsync();

        weeklyRepeatMeals = weeklyRepeatMeals
            .Where(m => m.StartTime!.Value.DayOfWeek == date.DayOfWeek)
            .ToList();

        var meals = exactDateMeals
            .Concat(weeklyRepeatMeals)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .OrderBy(m => m.StartTime)
            .ToList();

            return meals;
    }
}