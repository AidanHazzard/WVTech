using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[Table("UserFoodPreference")]
[PrimaryKey(nameof(UserId), nameof(TagId))]
public class UserFoodPreference
{
    public string UserId { get; set; } = null!;
    public int TagId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public Tag Tag { get; set; } = null!;
}
