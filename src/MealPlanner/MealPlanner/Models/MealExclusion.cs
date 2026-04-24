using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("MealExclusion")]
public class MealExclusion
{
    public int MealId { get; set; }

    // The specific date this repeat occurrence is suppressed
    public DateTime ExclusionDate { get; set; }

    [ForeignKey("MealId")]
    public Meal Meal { get; set; } = null!;
}
