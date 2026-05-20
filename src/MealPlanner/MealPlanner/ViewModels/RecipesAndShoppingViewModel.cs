using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class RecipesAndShoppingViewModel
{
    public IEnumerable<RecipeViewModel> Recipes { get; set; } = [];
    public IEnumerable<ShoppingListItem> ShoppingItems { get; set; } = [];
}
