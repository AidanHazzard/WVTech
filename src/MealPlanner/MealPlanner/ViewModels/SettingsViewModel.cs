namespace MealPlanner.ViewModels;

public class SettingsViewModel
{
    public List<string> CurrentFoodPrefs { get; set; } = [];
    public List<string> AvailableTags { get; set; } = [];
    public List<string> NewPreferences { get; set; } = [];

    public int? CalorieTarget { get; set; }
    public int? ProteinTarget { get; set; }
    public int? CarbTarget { get; set; }
    public int? FatTarget { get; set; }

    public List<DietaryRestrictionOptionViewModel> Restrictions { get; set; } = [];

    public string ActiveSection { get; set; } = "profile";

    public string FullName { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
    public string? DisplayHandle { get; set; }
}
