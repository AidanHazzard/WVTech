namespace MealPlanner.Models;

public class ShoppingListItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}