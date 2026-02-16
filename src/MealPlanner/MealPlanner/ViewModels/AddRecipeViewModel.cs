using System.ComponentModel.DataAnnotations;
public class AddRecipeViewModel
{
    public string Name { get; set; }

    public List<string> Ingredients { get; set; }

    public string Directions { get; set; }



    //loops through edge cases to look for errors
    public bool AnyErrors()
    {
        if (Directions == null || Directions.Trim() == "" ||
            Ingredients == null || Ingredients.Count == 0 ||
            Name == null || Name.Trim() == "")
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
}