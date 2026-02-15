using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("Recipe")]
public class Recipe
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Directions { get; set; }
    public string Ingredients { get; set; }

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; } 

    public List<Meal> Meals { get; set; } = [];
}
