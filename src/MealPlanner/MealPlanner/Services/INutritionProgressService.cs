namespace MealPlanner.Services;

public interface INutritionProgressService
{
     Task<NutritionProgressDto> GetDailyProgressAsync(string userId, DateOnly day);

    Task<NutritionProgressDto> GetRangeProgressAsync(string userId, DateOnly startDay, DateOnly endDay);
}

public record NutritionProgressDto(
    string UserId,
    DateOnly StartDay,
    DateOnly EndDay,
    MacroTargets Targets,
    MacroTotals Totals
);

public record MacroTargets(
    int Calories,
    int Protein,
    int Carbs,
    int Fat
);

public record MacroTotals(
    int Calories,
    int Protein,
    int Carbs,
    int Fat
);