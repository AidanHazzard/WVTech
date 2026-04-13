using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserNutritionPreferenceRepository
{
    public Task<UserNutritionPreference?> GetUsersNutritionPreferenceAsync(string userId);
}