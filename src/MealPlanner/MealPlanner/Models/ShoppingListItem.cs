namespace MealPlanner.Models;

public class ShoppingListItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;
}