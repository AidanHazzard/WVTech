using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IMealRepository : IRepository<Meal>
{
    Task<List<Meal>> GetUserMealsByDateAsync(User user, DateTime date);
}