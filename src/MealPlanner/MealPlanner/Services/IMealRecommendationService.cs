using MealPlanner.Models;
using MealPlanner.ViewModels;

namespace MealPlanner.Services;

public interface IMealRecommendationService
{
    public Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate);
    public Task<List<Meal>> GetRecommendedDayPlanForUser(User user, DateTime mealDate, DayPlanConfigViewModel config);
}