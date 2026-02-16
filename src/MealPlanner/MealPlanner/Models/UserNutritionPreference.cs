using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserNutritionPreference")]
public class UserNutritionPreference
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } 

    public int? CalorieTarget { get; set; }

    public int? ProteinTarget { get; set; }

    public int? CarbTarget { get; set; }

    public int? FatTarget { get; set; }
}
