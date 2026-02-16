using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserDietaryRestriction")]
public class UserDietaryRestriction
{
<<<<<<< HEAD
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
=======
    public int UserId { get; set; }
>>>>>>> 2b2fd8c (In the UserDietaryRestriction.cs file, I added 'DietaryRestrictionId' to the join model in order to be able to store a certain restriction)
    public User User { get; set; }
    public int DietaryRestrictionId { get; set; }
    public DietaryRestriction DietaryRestriction { get; set; }

}
