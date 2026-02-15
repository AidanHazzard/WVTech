using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class CreateMealViewModel
{
    [Required]
    [MinLength(1, ErrorMessage = "No Recipes Added")]
    public List<int> RecipeIds { get; set; } = [];
}