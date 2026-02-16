using MealPlanner.Services;

namespace MealPlanner.ViewModels;

public class NutritionProgessViewModel
{
    public NutritionProgressDto Progress { get; set; }

    public NutritionProgessViewModel(NutritionProgressDto progress)
    {
        Progress = progress;
    }
}