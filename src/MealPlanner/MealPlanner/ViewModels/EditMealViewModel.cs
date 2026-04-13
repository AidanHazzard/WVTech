using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels
{
    public class EditMealViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(1, 12)]
        public int SelectedMonth { get; set; }

        [Required]
        [Range(1, 31)]
        public int SelectedDay { get; set; }

        public bool RepeatWeekly { get; set; }

        public List<int> RecipeIds { get; set; } = new List<int>();

        public List<RecipeDisplayViewModel> Recipes { get; set; } = new List<RecipeDisplayViewModel>();
    }

    public class RecipeDisplayViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}