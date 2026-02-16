namespace MealPlanner.Services;

public interface INutritionProgressService
{
    Task<NutritionProgressDto> GetDailyProgressAsync(string userId, DateOnly day);

    Task<NutritionProgressDto> GetRangeProgressAsync(string userId, DateOnly startDay, DateOnly endDay);
}