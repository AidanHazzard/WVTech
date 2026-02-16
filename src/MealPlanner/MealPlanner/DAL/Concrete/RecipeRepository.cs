using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.DAL.Concrete;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(MealPlannerDBContext context)
        : base(context)
    {
    }

    public List<Recipe> GetRecipesByName(string name)
    {        
        List<Recipe> results = _dbset.Where(
            r => r.Name.ToLower().Contains($" {name.ToLower()}") || r.Name.ToLower().StartsWith($"{name.ToLower()}")
            ).ToList();
        
        return results;
    }
}