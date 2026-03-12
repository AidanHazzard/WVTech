using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[Table("IngredientBase")]
[Index(nameof(Name), IsUnique = true)]
public class IngredientBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    public override int GetHashCode()
    {
        return this.Name.GetHashCode();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not IngredientBase)
        {
            return false;
        }
        if (this.Name != ((IngredientBase) obj).Name)
        {
            return false;
        }

        return true;
    }
}