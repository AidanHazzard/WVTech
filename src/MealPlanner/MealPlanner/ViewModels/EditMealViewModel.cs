using MealPlanner.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels
{
    public class EditMealViewModel
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        public bool RepeatWeekly { get; set; }

        // <- This is the property your controller expects
        public List<int> RecipeIds { get; set; } = new List<int>();
    }
}