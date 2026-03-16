using MealPlanner.Models;
using MealPlanner.Models.DTO;

namespace MealPlanner.Services;

public interface IExternalRecipeService
{
    public Task<IEnumerable<RecipeDTO>> SearchExternalRecipesByName(string recipeName);
    public Task<Recipe?> GetExternalRecipeByURI(string URI);
}