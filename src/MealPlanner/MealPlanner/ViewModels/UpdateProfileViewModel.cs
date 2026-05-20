namespace MealPlanner.ViewModels;

public class UpdateProfileViewModel
{
    public string FullName { get; set; } = "";
    public string? DisplayHandle { get; set; }
    public string? PhotoData { get; set; }
    public bool RemovePhoto { get; set; }
}
