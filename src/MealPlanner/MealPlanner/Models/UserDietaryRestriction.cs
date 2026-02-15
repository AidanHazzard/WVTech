using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserDietaryRestriction")]
public class UserDietaryRestriction
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int DietaryRestrictionId { get; set; }
    public DietaryRestriction DietaryRestriction { get; set; }

}
