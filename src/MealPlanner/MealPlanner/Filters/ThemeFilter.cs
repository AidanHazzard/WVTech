using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MealPlanner.Filters;

public class ThemeFilter : IAsyncActionFilter
{
    private readonly IUserSettingsRepository _userProfileRepository;
    private readonly UserManager<User> _userManager;

    public ThemeFilter(IUserSettingsRepository userProfileRepository, UserManager<User> userManager)
    {
        _userProfileRepository = userProfileRepository;
        _userManager = userManager;
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
                context.HttpContext.Items["ProfilePictureUrl"] = profile.ProfilePictureUrl ?? "";
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.FullName))
            {
                var parts = user.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var navInitials = parts.Length >= 2
                    ? $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}"
                    : char.ToUpper(parts[0][0]).ToString();
                context.HttpContext.Items["NavInitials"] = navInitials;
            }
        }

        await next();
    }
}