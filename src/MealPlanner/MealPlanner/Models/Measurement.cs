using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[Table("Measurement")]
[Index(nameof(Name), IsUnique = true)]
public class Measurement
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(32)]
    public string Name { get; set; }

    public override int GetHashCode()
    {
        return this.Name.GetHashCode();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Measurement)
        {
            return false;
        }
        if (this.Name != ((Measurement) obj).Name)
        {
            return false;
        }

        return true;
    }
}