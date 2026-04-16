using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class CreateMealViewModel
{
    public List<int> RecipeIds { get; set; } = [];

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int SelectedMonth { get; set; }

    [Required]
    [Range(1, 31)]
    public int SelectedDay { get; set; }

    public bool RepeatWeekly { get; set; }
}