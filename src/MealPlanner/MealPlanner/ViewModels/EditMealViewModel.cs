using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels
{
    public class EditMealViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        public string? Time { get; set; }

        public int SelectedMonth { get; set; }

        public int SelectedDay { get; set; }

        public bool RepeatWeekly { get; set; }

        public List<DayOfWeek> RepeatDays { get; set; } = new List<DayOfWeek>();

        public List<int> RecipeIds { get; set; } = new List<int>();

        public List<RecipeDisplayViewModel> Recipes { get; set; } = new List<RecipeDisplayViewModel>();
    }

    public class RecipeDisplayViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}