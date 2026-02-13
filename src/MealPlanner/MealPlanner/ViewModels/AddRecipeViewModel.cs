using System.ComponentModel.DataAnnotations;
public class AddRecipeViewModel
{
    [Required(ErrorMessage = "Recipe name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "At least one ingredient is required")]
    public List<string> Ingredients { get; set; }

    [Required(ErrorMessage = "Instructions are required")]
    public string Directions { get; set; }

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
}