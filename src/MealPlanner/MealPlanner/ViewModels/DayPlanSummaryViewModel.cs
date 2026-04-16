using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class DayPlanMealViewModel
{
    public int MealId { get; set; }
    public bool IsSnack { get; set; }
    public MealSize Size { get; set; }
    public List<Recipe> Recipes { get; set; } = [];
    public MealPreferenceViewModel OriginalPreferences { get; set; } = new();
}

public class DayPlanSummaryViewModel
{
    public DateTime Date { get; set; }
    public List<DayPlanMealViewModel> Meals { get; set; } = [];
}
