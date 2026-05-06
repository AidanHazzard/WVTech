using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class PantryRepository : IPantryRepository
{
    private readonly MealPlannerDBContext _context;

    public PantryRepository(MealPlannerDBContext context)
    {
        _context = context;
    }

    public List<Ingredient> GetByUserId(string userId)
    {
        return _context.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.PantryItems)
            .ToList();
    }

    public void RemoveItem(int ingredientId, string userId)
    {
        var user = _context.Set<User>()
            .Include(u => u.PantryItems)
            .FirstOrDefault(u => u.Id == userId);

        var item = user?.PantryItems.FirstOrDefault(i => i.Id == ingredientId);
        if (item != null)
            _context.Set<Ingredient>().Remove(item);
    }
}
