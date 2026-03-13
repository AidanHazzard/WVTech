using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("Meal")]
public class Meal
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }  

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? RepeatRule { get; set; }
    
    public List<Recipe> Recipes { get; set; } = [];


    public void UpdateFromEdit(Meal editedMeal, IEnumerable<Recipe> selectedRecipes)
    {
        this.StartTime = editedMeal.StartTime;
        this.EndTime = editedMeal.EndTime;
        this.RepeatRule = editedMeal.RepeatRule;

        this.Recipes.Clear();
        if (selectedRecipes != null)
        {
            this.Recipes.AddRange(selectedRecipes);
        }
    }
    
}
