using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("MealAutoRemovedIngredient")]
[PrimaryKey(nameof(MealId), nameof(CompletionDate), nameof(IngredientBaseId))]
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
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Meal Meal { get; set; } = null!;

    [ForeignKey("IngredientBaseId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public IngredientBase IngredientBase { get; set; } = null!;

    [ForeignKey("MeasurementId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Measurement Measurement { get; set; } = null!;
}
