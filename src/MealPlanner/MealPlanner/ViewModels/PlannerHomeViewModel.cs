using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class PlannerHomeViewModel
{
    public DateTime SelectedDate { get; set; }
    public List<Meal> Meals { get; set; } = [];
    public NutritionBarInfoViewModel? NutritionBar { get; set; }
}