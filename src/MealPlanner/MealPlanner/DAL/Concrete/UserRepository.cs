using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly MealPlannerDBContext _context;

    public UserRepository(MealPlannerDBContext context) : base(context)
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
}
