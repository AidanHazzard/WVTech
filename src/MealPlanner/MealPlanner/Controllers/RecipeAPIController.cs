using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Controllers;

[ApiController]
[Route("api/recipe")]
public class RecipeAPIController : ControllerBase
{
    private readonly IRecipeRepository _recipeRepo;

    public RecipeAPIController(IRecipeRepository recipeRepo)
    {
        _recipeRepo = recipeRepo;
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Recipe>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SearchRecipesByName(string name)
    {
        IEnumerable<Recipe> results = _recipeRepo.GetRecipesByName(name);
        if (results.IsNullOrEmpty())
        {
            return NotFound();
        }
        return Ok(results);
    }
}