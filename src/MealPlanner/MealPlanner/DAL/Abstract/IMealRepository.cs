using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IMealRepository : IRepository<Meal>
{
    Task<List<Meal>> GetUserMealsByDateAsync(User user, DateTime date);
    Task<List<Meal>> GetUserMealsByDateRangeAsync(User user, DateTime start, DateTime end);
    Task<List<Meal>> GetDistinctUserMealsAsync(User user);
    Task<Meal> ReadAsync(int id);
    Task<List<Meal>> GetMealsByIdsAsync(IEnumerable<int> ids);
    Task LoadRecipesAsync(Meal meal);
    Task UpdateMealAsync(Meal meal, Meal updatedData, IEnumerable<int> recipeIds);
}