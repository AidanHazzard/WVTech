using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserDietaryRestriction")]
public class UserDietaryRestriction
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }

}
