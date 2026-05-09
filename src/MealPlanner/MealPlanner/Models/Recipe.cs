using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[Table("Recipe")]
[Index(nameof(ExternalUri), IsUnique = true)]
public class Recipe
{
    private static readonly HashSet<string> AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Directions { get; set; }
    public List<Ingredient> Ingredients { get; set; } = [];

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; }
    public string? ExternalUri { get; set; }
    public string? ImageUrl { get; set; }
    public List<Meal> Meals { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public List<User> Users { get; } = [];
    public List<UserRecipe> UserRecipes { get; } = [];

    public async Task SaveImageAsync(IFormFile? imageFile, string webRootPath)
    {
        if (imageFile == null || imageFile.Length == 0) return;
        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext)) return;

        var dir = Path.Combine(webRootPath, "images", "recipes");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
        await imageFile.CopyToAsync(stream);
        ImageUrl = $"/images/recipes/{fileName}";
    }
}
