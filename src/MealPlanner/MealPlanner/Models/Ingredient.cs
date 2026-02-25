using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("Ingredient")]
public class Ingredient
{
    [Key]
    public int Id { get; set; }

    [Required]
    public IngredientBase IngredientBase { get; set; }

    [Required]
    public Measurement Measurement { get; set; }
    public float Amount { get; set; }
}