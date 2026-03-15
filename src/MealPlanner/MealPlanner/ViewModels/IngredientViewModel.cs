using System.ComponentModel.DataAnnotations;
using MealPlanner.Models;

namespace MealPlanner.ViewModels;

public class IngredientViewModel
{
    public IngredientViewModel()
    {}

    public IngredientViewModel(Ingredient ingredient)
    {
        Amount = ingredient.Amount;
        IngredientBase = ingredient.IngredientBase.Name;
        Measurement = ingredient.Measurement.Name;
    }

    public float Amount;

    [Required(ErrorMessage = "Please provide an ingredient name")]
    public string IngredientBase;

    [Required(ErrorMessage = "Please select a measurement")]
    public string Measurement;
}