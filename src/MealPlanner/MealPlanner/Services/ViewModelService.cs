using MealPlanner.Models;
using MealPlanner.ViewModels;

namespace MealPlanner.Services;

public class ViewModelService
{
    public static RecipeViewModel RecipeToRecipeVM(Recipe recipe)
    {
        return new RecipeViewModel(recipe);
    }

    public static Recipe RecipeFromRecipeVM(RecipeViewModel vm)
    {
        Recipe recipe = new Recipe()
        {
            Name = vm.Name,
            Ingredients = [],
            Directions = vm.Directions,
            Calories = vm.Calories,
            Protein = vm.Protein,
            Carbs = vm.Carbs,
            Fat = vm.Fat,
            Meals = []
        };

        // Adds the ingredients to the recipe
        // TODO: Change recipe view model to use IngredientViewModel or a dictionary
        for (int i = 0; i < vm.Ingredients.Count; i++)
        {
            recipe.Ingredients.Add(new Ingredient
            {
                Amount = vm.IngredientAmounts[i],
                IngredientBase = new IngredientBase { Name = vm.Ingredients[i] },
                Measurement = new Measurement{ Name = vm.IngredientMeasurements[i] }
            });
        }

        recipe.Tags = vm.Tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => new Tag { Name = t.Trim() })
            .ToList();

        return recipe;
    }

    public static Recipe EditRecipeVMToModel(Recipe recipeFromDataBase, RecipeViewModel vm)
    {
        Recipe updated = RecipeFromRecipeVM(vm);

        recipeFromDataBase.Ingredients.Clear();
        recipeFromDataBase.Name = updated.Name;
        recipeFromDataBase.Directions = updated.Directions;
        recipeFromDataBase.Calories = updated.Calories;
        recipeFromDataBase.Protein = updated.Protein;
        recipeFromDataBase.Carbs = updated.Carbs;
        recipeFromDataBase.Fat = updated.Fat;
        recipeFromDataBase.Ingredients = updated.Ingredients;
        recipeFromDataBase.Tags.Clear();
        recipeFromDataBase.Tags.AddRange(updated.Tags);
        return recipeFromDataBase;
    }
}