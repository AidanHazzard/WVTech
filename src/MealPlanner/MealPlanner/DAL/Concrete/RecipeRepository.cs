using MealPlanner.Models;

namespace MealPlanner.DAL.Concrete;

public class RecipeRepository : Repository<Recipe>
{
    public RecipeRepository(MealPlannerDBContext context)
        : base(context)
    {
    }

    public IQueryable<Recipe> GetRecipesByName(string name)
    {        
        IQueryable<Recipe> results = _dbset.Where(
            r => r.Name.ToLower().Contains($" {name.ToLower()}")
            );
                
        results = results.Concat(_dbset.Where(
            r => r.Name.ToLower().StartsWith($"{name.ToLower()}")
            )
        );
        
        return results;
    }
}