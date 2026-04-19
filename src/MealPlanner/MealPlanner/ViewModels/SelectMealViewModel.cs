using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class SelectMealViewModel
{
    public DateTime SelectedDate { get; set; }
    public List<Meal> Meals { get; set; } = [];
}
