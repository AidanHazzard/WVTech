using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserFavoriteRecipe")]
public class UserFavoriteRecipe
{
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}