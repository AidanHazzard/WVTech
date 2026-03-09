using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class CreateMealViewModel
{
    public List<int> RecipeIds { get; set; } = [];

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^.+\s+.+$", ErrorMessage = "Title must contain at least two words.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    public bool RepeatWeekly { get; set; }
}