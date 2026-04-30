using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models;

public class ShoppingListItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int IngredientBaseId { get; set; }
    public IngredientBase IngredientBase { get; set; } = null!;

    public int MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public float Amount { get; set; }

    public bool IsAutoAdded { get; set; }
}
