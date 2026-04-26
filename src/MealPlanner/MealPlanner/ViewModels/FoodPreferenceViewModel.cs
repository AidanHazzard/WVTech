namespace MealPlanner.ViewModels;

public class FoodPreferenceViewModel
{
    public List<string> CurrentPreferences { get; set; } = [];
    public List<string> AvailableTags { get; set; } = [];
    public List<string> NewPreferences { get; set; } = [];
}
