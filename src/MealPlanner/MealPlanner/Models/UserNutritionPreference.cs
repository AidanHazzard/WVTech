using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Models;

[Table("UserNutritionPreference")]
public class UserNutritionPreference
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } 

    public int? CalorieTarget { get; set; }

    public int? ProteinTarget { get; set; }

    public int? CarbTarget { get; set; }

    public int? FatTarget { get; set; }

    /// <summary>
    /// Returns the MealSize whose calorie threshold is closest to the meal's total calories.
    /// When CalorieTarget is set, the thresholds scale proportionally against a 2000 cal/day reference
    /// so that the same absolute meal size reads differently for users with different daily goals.
    /// </summary>
    public MealSize GetMealSize(Meal meal)
    {
        const int referenceDaily = 2000;
        double scale = CalorieTarget.HasValue ? (double)CalorieTarget.Value / referenceDaily : 1.0;
        int mealCalories = meal.Recipes.Sum(r => r.Calories);

        return Enum.GetValues<MealSize>()
            .OrderBy(s => Math.Abs(mealCalories - s.Calories() * scale))
            .First();
    }
}
