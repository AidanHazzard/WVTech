using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class UserSettingsRepository : Repository<UserProfile>, IUserSettingsRepository
{
    private readonly MealPlannerDBContext _context;

    public UserSettingsRepository(MealPlannerDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        return await _dbset.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task ToggleDarkThemeAsync(string userId)
    {
        var profile = await GetByUserIdAsync(userId);
        if (profile != null)
        {
            if (profile.IsDarkTheme)
            {
                profile.IsDarkTheme = false;
            }
            else
            {
                profile.IsDarkTheme = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}