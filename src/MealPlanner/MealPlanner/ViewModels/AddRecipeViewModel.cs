public class AddRecipeViewModel
{
    public string Name { get; set; }
    public List<string> Ingredients { get; set; }
    public string Steps { get; set; }

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