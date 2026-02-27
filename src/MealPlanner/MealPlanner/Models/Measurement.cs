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
}