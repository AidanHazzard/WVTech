using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("Meal")]
public class Meal
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? RepeatRule { get; set; }

    public List<Recipe> Recipes { get; set; } = [];

    public void UpdateFromEdit(Meal editedMeal, IEnumerable<Recipe> selectedRecipes)
    {
        StartTime = editedMeal.StartTime;
        EndTime = editedMeal.EndTime;
        RepeatRule = editedMeal.RepeatRule;

        Recipes.Clear();
        if (selectedRecipes != null)
        {
            Recipes.AddRange(selectedRecipes);
        }
    }
}