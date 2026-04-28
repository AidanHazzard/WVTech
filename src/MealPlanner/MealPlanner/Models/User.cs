using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    
    public List<Recipe> Recipes { get; } = [];
    public List<UserRecipe> UserRecipes { get; } = [];
    public List<Ingredient> PantryItems { get; set; } = [];
}