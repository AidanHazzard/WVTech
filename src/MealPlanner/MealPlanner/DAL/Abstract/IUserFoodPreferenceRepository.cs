namespace MealPlanner.DAL.Abstract;

public interface IUserFoodPreferenceRepository
{
    Task<List<string>> GetFoodPreferenceNamesAsync(string userId);
    Task AddFoodPreferencesAsync(string userId, List<string> tagNames);
    Task RemoveFoodPreferenceAsync(string userId, string tagName);
}
