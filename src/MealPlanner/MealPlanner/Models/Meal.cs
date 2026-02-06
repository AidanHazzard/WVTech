using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("Meal")]
public class Meal
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? RepeatRule { get; set; }
    public List<Recipe> Recipes { get; set; } = [];
}
