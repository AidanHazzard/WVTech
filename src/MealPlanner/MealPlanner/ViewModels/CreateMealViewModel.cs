using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class CreateMealViewModel
{
    public List<int> RecipeIds { get; set; } = [];

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; }

    public bool RepeatWeekly { get; set; }
}