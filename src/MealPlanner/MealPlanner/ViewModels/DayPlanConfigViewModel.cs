using System.ComponentModel.DataAnnotations;
using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class MealPreferenceViewModel
{
    public MealSize Size { get; set; } = MealSize.Average;
    public List<int> TagIds { get; set; } = [];
}

public class DayPlanConfigViewModel
{
    [Required]
    [Range(1, 10)]
    public int MealCount { get; set; }

    public bool IncludeSnacks { get; set; }
    public MealSize? SnackSize { get; set; }

    public List<MealPreferenceViewModel> MealPreferences { get; set; } = [];

    [Required]
    [Range(1, 12)]
    public int SelectedMonth { get; set; }

    [Required]
    [Range(1, 31)]
    public int SelectedDay { get; set; }
}
