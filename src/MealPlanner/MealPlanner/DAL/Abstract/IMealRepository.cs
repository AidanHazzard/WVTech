using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IMealRepository : IRepository<Meal>
{
    Task<List<Meal>> GetUserMealsByDateAsync(User user, DateTime date);
    Task<Meal> ReadAsync(int id); 
    Task LoadRecipesAsync(Meal meal);
    Task UpdateMealAsync(Meal meal, Meal updatedData, IEnumerable<int> recipeIds);
}