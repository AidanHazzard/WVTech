using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("DietaryRestriction")]
public class DietaryRestriction
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(2048)]
    public string? Name { get; set; }
}
