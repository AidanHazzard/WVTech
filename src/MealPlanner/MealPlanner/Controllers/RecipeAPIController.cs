using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Models.DTO;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Controllers;

[ApiController]
[Route("api/recipe")]
public class RecipeAPIController : ControllerBase
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserRecipeRepository _userRecipeRepository;
    private readonly IRegistrationService _registrationSerivice;
    private readonly MealPlannerDBContext _context;

    public RecipeAPIController(
        MealPlannerDBContext context,
        IRecipeRepository recipeRepository,
        IUserRecipeRepository userRecipeRepository,
        IRegistrationService registrationService)
    {
        _context = context;
        _recipeRepository = recipeRepository;
        _userRecipeRepository = userRecipeRepository;
        _registrationSerivice = registrationService;
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RecipeDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchRecipesByName(string name)
    {
        List<RecipeDTO> results = _recipeRepository.GetRecipesByName(name).Select(r => new RecipeDTO(r)).ToList();
        if (results.IsNullOrEmpty())
        {
            return NotFound();
        }
        
        foreach (RecipeDTO r in results)
        {
            r.VotePercentage = await _userRecipeRepository.GetRecipeVotePercentage(r.Id);
        }

        results = results.OrderByDescending(r => r.VotePercentage).ToList();

        return Ok(results);
    }

    [HttpPut("vote")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRecipeVote(int recipeId, UserVoteType voteType)
    {
        User? user = await _registrationSerivice.FindUserByClaimAsync(User);
        if (user == null) return Forbid();

        Recipe? recipe = _recipeRepository.Read(recipeId);
        if (recipe == null) return NotFound();

        await _userRecipeRepository.ChangeRecipeVoteAsync(user, recipe, voteType);
        await _context.SaveChangesAsync();
        return Accepted();
    }

    [HttpGet("rating")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(float))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecipeRating(int recipeId)
    {
        Recipe? recipe = _recipeRepository.Read(recipeId);
        if (recipe == null) return NotFound();

        return Ok(await _userRecipeRepository.GetRecipeVotePercentage(recipeId));
    }
}