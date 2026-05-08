using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class PantryItemViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, float.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public float Amount { get; set; }

    [Required]
    public string Measurement { get; set; } = string.Empty;
}
