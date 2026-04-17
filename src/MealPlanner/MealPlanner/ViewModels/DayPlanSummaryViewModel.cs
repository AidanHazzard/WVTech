using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class DayPlanMealViewModel
{
    public int MealId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = [];
    public MealPreferenceViewModel OriginalPreferences { get; set; } = new();
}

public class DayPlanSummaryViewModel
{
    public DateTime Date { get; set; }
    public List<DayPlanMealViewModel> Meals { get; set; } = [];
    public List<Tag> AvailableTags { get; set; } = [];
}
