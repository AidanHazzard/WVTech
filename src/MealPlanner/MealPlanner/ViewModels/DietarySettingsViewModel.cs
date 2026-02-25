namespace MealPlanner.ViewModels
{
    public class DietarySettingsViewModel
    {
        public List<DietaryRestrictionOptionViewModel> Restrictions { get; set; } = new();
    }

    public class DietaryRestrictionOptionViewModel
    {
        public int DietaryRestrictionId { get; set; }
        public string Name { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}