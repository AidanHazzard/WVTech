namespace MealPlanner.Services;

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