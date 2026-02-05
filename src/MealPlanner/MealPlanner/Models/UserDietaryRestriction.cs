using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserDietaryRestriction")]
public class UserDietaryRestriction
{
    [Key]
    public int UserId { get; set; }
    public User User { get; set; }

}
