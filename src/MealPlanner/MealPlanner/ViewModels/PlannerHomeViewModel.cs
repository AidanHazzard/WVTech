using MealPlanner.Models;
using MealPlanner.Services;

namespace MealPlanner.ViewModels;

public class PlannerHomeViewModel
{
    public DateTime SelectedDate { get; set; }
    public List<Meal> Meals { get; set; } = [];
    //public NutritionProgressDto? DailyNutrition { get; set; }
}