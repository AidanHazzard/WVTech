using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserProfile")]
public class UserProfile
{
    [Key]
    public int Id { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? age { get; set; }
    public decimal? HeightInInches { get; set; }
    public decimal? Weight { get; set; }
    public string? ActivityLevel { get; set; }
    public string? ProfilePictureUrl { get; set; }

}
