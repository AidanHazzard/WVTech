using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[Table("Recipe")]
[Index(nameof(ExternalUri), IsUnique = true)]
public class Recipe
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Directions { get; set; }
    public List<Ingredient> Ingredients { get; set; } = [];

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; } 
    public string? ExternalUri { get; set; }
    public List<Meal> Meals { get; set; } = [];
    public List<User> Users { get; } = [];
    public List<UserRecipe> UserRecipes { get; } = [];
}
