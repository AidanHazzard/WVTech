using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class PantryRepository : Repository<User>, IPantryRepository
{
    private readonly MealPlannerDBContext _context;

    public PantryRepository(MealPlannerDBContext context) : base(context)
    {
        _context = context;
    }

    public List<Ingredient> GetByUserId(string userId)
    {
        return _dbset
            .Where(u => u.Id == userId)
            .SelectMany(u => u.PantryItems)
            .ToList();
    }

    public void RemoveItem(int ingredientId, string userId)
    {
        var user = _dbset
            .Include(u => u.PantryItems)
            .FirstOrDefault(u => u.Id == userId);

        var item = user?.PantryItems.FirstOrDefault(i => i.Id == ingredientId);
        if (item != null)
            _context.Set<Ingredient>().Remove(item);
    }

    public void UpdateItemAmount(int ingredientId, string userId, float newAmount)
    {
        var user = _dbset
            .Include(u => u.PantryItems)
            .FirstOrDefault(u => u.Id == userId);

        var item = user?.PantryItems.FirstOrDefault(i => i.Id == ingredientId);
        if (item != null)
            item.Amount = newAmount;
    }

    public HashSet<int> GetUserIngredientBaseIds(string userId)
    {
        return _dbset
            .Where(u => u.Id == userId)
            .SelectMany(u => u.PantryItems)
            .Select(i => i.IngredientBase.Id)
            .ToHashSet();
    }

    public List<Ingredient> GetMatchingPantryItems(string userId, HashSet<int> ingredientBaseIds)
    {
        return _dbset
            .Where(u => u.Id == userId)
            .SelectMany(u => u.PantryItems)
            .Where(i => ingredientBaseIds.Contains(i.IngredientBase.Id))
            .ToList();
    }

    public void AddItem(string userId, Ingredient item)
    {
        var user = _dbset
            .Include(u => u.PantryItems)
            .FirstOrDefault(u => u.Id == userId);
        user?.PantryItems.Add(item);
    }

    public void AddAutoRemovedIngredients(List<MealAutoRemovedIngredient> records)
    {
        _context.MealAutoRemovedIngredients.AddRange(records);
    }

    public List<MealAutoRemovedIngredient> GetAutoRemovedIngredients(int mealId, DateTime completionDate)
    {
        return _context.MealAutoRemovedIngredients
            .Include(r => r.IngredientBase)
            .Include(r => r.Measurement)
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
    }

    public void RemoveAutoRemovedIngredients(int mealId, DateTime completionDate)
    {
        var records = _context.MealAutoRemovedIngredients
            .Where(r => r.MealId == mealId && r.CompletionDate == completionDate.Date)
            .ToList();
        _context.MealAutoRemovedIngredients.RemoveRange(records);
    }

    public void PurgeExpiredAutoRemovedIngredients(DateTime cutoff)
    {
        var expired = _context.MealAutoRemovedIngredients
            .Where(r => r.CreatedAt < cutoff)
            .ToList();
        _context.MealAutoRemovedIngredients.RemoveRange(expired);
    }
}
