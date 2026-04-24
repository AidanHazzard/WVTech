using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("MealCompletion")]
public class MealCompletion
{
    public int MealId { get; set; }

    // Date-only (time component always midnight UTC)
    public DateTime CompletionDate { get; set; }

    [ForeignKey("MealId")]
    public Meal Meal { get; set; } = null!;
}
