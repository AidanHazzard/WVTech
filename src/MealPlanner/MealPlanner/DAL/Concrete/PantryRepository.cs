using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

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
}
