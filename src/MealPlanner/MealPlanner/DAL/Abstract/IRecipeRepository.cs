using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IRecipeRepository : IRepository<Recipe>
{
    public IQueryable<Recipe> GetRecipesByName(string name);
}