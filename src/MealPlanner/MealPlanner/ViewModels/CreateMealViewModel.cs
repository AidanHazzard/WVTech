using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class CreateMealViewModel
{
    public List<int> RecipeIds { get; set; } = [];

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; }

    public bool RepeatWeekly { get; set; }
}