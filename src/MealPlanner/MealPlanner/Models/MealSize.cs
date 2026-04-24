using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models;

public enum MealSize
{
    [Display(Name = "Small Snack")]
    SmallSnack,
    [Display(Name = "Small")]
    Small,
    [Display(Name = "Average")]
    Average,
    [Display(Name = "Large")]
    Large,
    [Display(Name = "Large Snack")]
    LargeSnack
}

public static class MealSizeExtensions
{
    public static int Calories(this MealSize size) => size switch
    {
        MealSize.SmallSnack => 150,
        MealSize.Small      => 400,
        MealSize.Average    => 600,
        MealSize.Large      => 800,
        MealSize.LargeSnack => 350,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public static bool IsSnack(this MealSize size) =>
        size == MealSize.SmallSnack || size == MealSize.LargeSnack;
}
