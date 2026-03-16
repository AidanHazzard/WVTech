using MealPlanner.DAL.Abstract;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MealPlanner.Filters;

public class ThemeFilter : IAsyncActionFilter
{
    private readonly IUserSettingsRepository _userProfileRepository;

    public ThemeFilter(IUserSettingsRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile != null)
            {
                context.HttpContext.Items["Theme"] = profile.IsDarkTheme ? "dark" : "light";
            }
        }

        await next();
    }
}