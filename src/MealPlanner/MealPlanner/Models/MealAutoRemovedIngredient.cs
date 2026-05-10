using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("MealAutoRemovedIngredient")]
public class MealAutoRemovedIngredient
{
    public int MealId { get; set; }

    public DateTime CompletionDate { get; set; }

    public int IngredientBaseId { get; set; }

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public float Amount { get; set; }

    public int MeasurementId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("MealId")]
    public Meal Meal { get; set; } = null!;

    [ForeignKey("IngredientBaseId")]
    public IngredientBase IngredientBase { get; set; } = null!;

    [ForeignKey("MeasurementId")]
    public Measurement Measurement { get; set; } = null!;
}
