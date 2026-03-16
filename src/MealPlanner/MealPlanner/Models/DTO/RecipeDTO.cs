namespace MealPlanner.Models.DTO;

public class RecipeDTO
{
    public RecipeDTO(Recipe recipe)
    {
        Name = recipe.Name;
        Id = recipe.Id;
    }
    
    public string Name { get; set; }
    public int Id { get; set; }
    public float VotePercentage { get; set; }
}