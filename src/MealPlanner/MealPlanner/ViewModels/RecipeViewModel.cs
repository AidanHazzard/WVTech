using System.ComponentModel.DataAnnotations;
using MealPlanner.Models;
public class RecipeViewModel
{
    public string Name { get; set; }

    public List<string> Ingredients { get; set; }

    public string Directions { get; set; }

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; }


    // REFACTOR: Really should move these to an extension class, but that's a later me problem
    //loops through edge cases to look for errors
    public bool AnyErrors()
    {
        if (Directions == null || Directions.Trim() == "" ||
            Ingredients == null || Ingredients.Count == 0 ||
            Name == null || Name.Trim() == "" ||
            Calories < 0 || Protein < 0 || Carbs < 0 || Fat < 0)
        {
            return true;
        }
        foreach (string entry in Ingredients)
        {
            if (entry.Trim() == "")
            {
                return true;
            }
        }
        return false;
    }

    //creates a flattend string of all the entrys from the ingredients list
    //no need to error check the ingredient list because that will have already been ran above
    public string FlattenList()
    {
        string ingredientsFlat = "";
        for (int i = 0; i < Ingredients.Count; i++)
        {
            //if statemnt so the first entry doesnt have an \n above it
            if (i == 0)
            {
                ingredientsFlat = $"{Ingredients[i]}";
            }
            else
            {
                ingredientsFlat = $"{ingredientsFlat}\n{Ingredients[i]}";
            }
        }
        return ingredientsFlat;
    }

    public static RecipeViewModel FromRecipe(Recipe recipe)
    {
        // Unflatten Ingredients list
        List<string> ingredients = recipe.Ingredients.Split('\n').ToList();

        return new RecipeViewModel
        {
            Name = recipe.Name,
            Ingredients = ingredients,
            Directions = recipe.Directions,
            Calories = recipe.Calories,
            Protein = recipe.Protein,
            Carbs = recipe.Carbs,
            Fat = recipe.Fat
        };
    }
}