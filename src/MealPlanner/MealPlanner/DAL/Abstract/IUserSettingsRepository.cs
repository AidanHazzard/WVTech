using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserSettingsRepository : IRepository<UserProfile>
{
    public Task<UserProfile?> GetByUserIdAsync(string userId);
    public Task ToggleDarkThemeAsync(string userId);
}