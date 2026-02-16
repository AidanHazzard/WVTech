using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserDietaryRestriction")]
public class UserDietaryRestriction
{
    public string UserId { get; set; } = null!;
    public User User { get; set; }  = null!;

    public int DietaryRestrictionId { get; set; }
    public DietaryRestriction DietaryRestriction { get; set; } = null!;
}
