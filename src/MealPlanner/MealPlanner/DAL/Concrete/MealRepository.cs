using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class MealRepository : Repository<Meal>, IMealRepository
{
    private readonly MealPlannerDBContext _context;
    public MealRepository(MealPlannerDBContext context) : base(context)
    {
        _context = context;
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

    public async Task<List<Meal>> GetUserMealsByDateRangeAsync(User user, DateTime start, DateTime end)
    {
        var rangeEnd = end.AddDays(1);

        var exactMeals = await _dbset
            .Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.StartTime != null)
            .Where(m => m.StartTime >= start && m.StartTime < rangeEnd)
            .ToListAsync();

        var daysInRange = Enumerable.Range(0, (end.Date - start.Date).Days + 1)
            .Select(d => start.AddDays(d).DayOfWeek)
            .ToHashSet();

        var weeklyMeals = await _dbset
            .Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.RepeatRule == "Weekly" && m.StartTime != null)
            .ToListAsync();

        weeklyMeals = weeklyMeals
            .Where(m => daysInRange.Contains(m.StartTime!.Value.DayOfWeek))
            .ToList();

        return exactMeals
            .Concat(weeklyMeals)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .OrderBy(m => m.StartTime)
            .ToList();
    }

    public async Task<List<Meal>> GetDistinctUserMealsAsync(User user)
    {
        var userMeals = await _dbset
            .Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();

        return userMeals
            .GroupBy(m => m.Title)
            .Select(g => g.First())
            .OrderBy(m => m.Title)
            .ToList();
    }

    // Load recipes for a meal
    public async Task LoadRecipesAsync(Meal meal)
    {
        if (meal == null) return;
        await _context.Entry(meal).Collection(m => m.Recipes).LoadAsync();
    }

    // Update meal properties & recipes
    public async Task UpdateMealAsync(Meal meal, Meal updatedData, IEnumerable<int> recipeIds)
{
    // Update basic properties
    meal.StartTime = updatedData.StartTime;
    meal.RepeatRule = updatedData.RepeatRule;

    // Load current recipes
    await _context.Entry(meal).Collection(m => m.Recipes).LoadAsync();

    // Clear old recipes
    meal.Recipes.Clear();

    // Add selected recipes
    var recipes = await _context.Recipes
        .Where(r => recipeIds.Contains(r.Id))
        .ToListAsync();

    meal.Recipes.AddRange(recipes);

    await _context.SaveChangesAsync();
}

    public async Task<Meal> ReadAsync(int id)
    {
        return await _dbset.FindAsync(id);
    }

    public async Task<List<Meal>> GetMealsByIdsAsync(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        return await _dbset
            .Include(m => m.Recipes)
            .Where(m => idSet.Contains(m.Id))
            .ToListAsync();
    }
}