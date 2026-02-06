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



}
