using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Models;

public class User : IdentityUser
{
    public string FullName { get; set; }
    
    public List<Recipe> OwnedRecipes { get; set; } = [];
}