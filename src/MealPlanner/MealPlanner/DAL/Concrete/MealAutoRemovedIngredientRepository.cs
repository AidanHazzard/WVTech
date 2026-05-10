using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class MealAutoRemovedIngredientRepository : Repository<MealAutoRemovedIngredient>, IMealAutoRemovedIngredientRepository
{
    public MealAutoRemovedIngredientRepository(MealPlannerDBContext context) : base(context)
    {
    }

    public void AddRange(List<MealAutoRemovedIngredient> records)
    {
        _dbset.AddRange(records);
    }

    public List<MealAutoRemovedIngredient> GetByMealAndDate(int mealId, DateTime completionDate)
    {
        return _dbset
            .Include(r => r.IngredientBase)
            .Include(r => r.Measurement)
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
    }

    public void RemoveByMealAndDate(int mealId, DateTime completionDate)
    {
        var records = _dbset
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
        _dbset.RemoveRange(records);
    }

    public void PurgeExpired(DateTime cutoff)
    {
        var expired = _dbset
            .Where(r => r.CreatedAt < cutoff)
            .ToList();
        _dbset.RemoveRange(expired);
    }
}
