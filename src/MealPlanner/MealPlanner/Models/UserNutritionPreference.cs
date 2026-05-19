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
    /// Returns the MealSize whose Ratio() is closest to the meal's average macro ratio.
    /// For each macro target that is set, the meal's total for that macro is divided by
    /// the daily target to get a fraction; those fractions are averaged across all set targets.
    /// Falls back to mealCalories / 2000 when no targets are set.
    /// </summary>
    public MealSize GetMealSize(Meal meal)
    {
        int mealCalories = meal.Recipes.Sum(r => r.Calories);
        int mealProtein  = meal.Recipes.Sum(r => r.Protein);
        int mealCarbs    = meal.Recipes.Sum(r => r.Carbs);
        int mealFat      = meal.Recipes.Sum(r => r.Fat);

        var ratios = new List<double>();
        if (CalorieTarget is > 0) ratios.Add((double)mealCalories / CalorieTarget.Value);
        if (ProteinTarget is > 0) ratios.Add((double)mealProtein  / ProteinTarget.Value);
        if (CarbTarget    is > 0) ratios.Add((double)mealCarbs    / CarbTarget.Value);
        if (FatTarget     is > 0) ratios.Add((double)mealFat      / FatTarget.Value);

        double mealRatio = ratios.Count > 0 ? ratios.Average() : mealCalories / 2000.0;

        return Enum.GetValues<MealSize>()
            .OrderBy(s => Math.Abs(mealRatio - s.Ratio()))
            .First();
    }
}
