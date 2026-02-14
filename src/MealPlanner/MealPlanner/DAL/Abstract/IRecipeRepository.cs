using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IRecipeRepository : IRepository<Recipe>
{
    public List<Recipe> GetRecipesByName(string name);
}