using MealPlanner.Services;

namespace MealPlanner.ViewModels;

public class NutritionSummaryViewModel
{
    public string ActiveTab { get; set; } = "weekly";
    public MacroTargets DailyTargets { get; set; } = null!;
    public List<DailyNutritionDto> AllDays { get; set; } = [];
    public NutritionBarInfoViewModel TodayBar { get; set; } = null!;
}
