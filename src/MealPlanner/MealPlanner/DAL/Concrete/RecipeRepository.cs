using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    DbSet<IngredientBase> _ingredientBaseSet;
    DbSet<Measurement> _measurementSet;

    public RecipeRepository(MealPlannerDBContext context)
        : base(context)
    {
        _ingredientBaseSet = context.Set<IngredientBase>();
        _measurementSet = context.Set<Measurement>();
    }

    public List<Recipe> GetRecipesByName(string name)
    {        
        List<Recipe> results = _dbset.Where(
            r => r.Name.ToLower().Contains($" {name.ToLower()}") || r.Name.ToLower().StartsWith($"{name.ToLower()}")
            ).ToList();
        
        return results;
    }

    public override void CreateOrUpdate(Recipe recipe)
    {
        foreach (Ingredient i in recipe.Ingredients)
        {
            if (i.IngredientBase.Id == 0)
            {
                // Use existing db entry for IngredientBase if it exits, ensuring Unique constraint
                try
                {
                    var found = _ingredientBaseSet.Where(b => b.Name == i.IngredientBase.Name).First();
                    i.IngredientBase = found ?? i.IngredientBase;
                }
                catch (InvalidOperationException)
                {
                    _ingredientBaseSet.Add(i.IngredientBase);
                }
            }

            if (i.Measurement.Id == 0)
            {
                // Use existing db entry for Measurement if it exits, ensuring Unique constraint
                try
                {
                    var found = _measurementSet.Where(m => m.Name == i.Measurement.Name).First();
                    i.Measurement = found;
                }
                catch (InvalidOperationException)
                {
                    _measurementSet.Add(i.Measurement);
                }
            }
        }
        base.CreateOrUpdate(recipe);
    }

    public async Task<Recipe?> ReadRecipeWithIngredientsAsync(int id)
    {
        return await _dbset
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}