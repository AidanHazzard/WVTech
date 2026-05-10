using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class MealAutoRemovedIngredientRepository : IMealAutoRemovedIngredientRepository
{
    private readonly MealPlannerDBContext _context;

    public MealAutoRemovedIngredientRepository(MealPlannerDBContext context)
    {
        _context = context;
    }

    public void AddRange(List<MealAutoRemovedIngredient> records)
    {
        _context.MealAutoRemovedIngredients.AddRange(records);
    }

    public List<MealAutoRemovedIngredient> GetByMealAndDate(int mealId, DateTime completionDate)
    {
        return _context.MealAutoRemovedIngredients
            .Include(r => r.IngredientBase)
            .Include(r => r.Measurement)
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
    }

    public void RemoveByMealAndDate(int mealId, DateTime completionDate)
    {
        var records = _context.MealAutoRemovedIngredients
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
        _context.MealAutoRemovedIngredients.RemoveRange(records);
    }

    public void PurgeExpired(DateTime cutoff)
    {
        var expired = _context.MealAutoRemovedIngredients
            .Where(r => r.CreatedAt < cutoff)
            .ToList();
        _context.MealAutoRemovedIngredients.RemoveRange(expired);
    }
}
