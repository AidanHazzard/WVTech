using MealPlanner.Services;

namespace MealPlanner.ViewModels;

public class NutritionReportViewModel
{
    public string ActiveTab { get; set; } = "weekly";
    public NutritionProgressDto Progress { get; set; } = null!;
}
