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

    public int? IronTarget { get; set; }

    public int? FiberTarget { get; set; }

    public int? CalciumTarget { get; set; }

    public int? VitaminATarget { get; set; }

    public int? VitaminCTarget { get; set; }

    public int? B12Target { get; set; }

    public int? FolateTarget { get; set; }
    
    public int? PotassiumTarget { get; set; }
}
