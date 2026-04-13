using MealPlanner.Models;

namespace MealPlanner.Services;

public interface IMealRecommendationService
{
    public Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime mealDate);
}