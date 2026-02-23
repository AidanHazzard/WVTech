using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers
{
    [Authorize]
    public class UserSettingsController : Controller
    {
        private readonly MealPlannerDBContext _db;

        public UserSettingsController(MealPlannerDBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Dietary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allRestrictions = await _db.DietaryRestrictions
                .OrderBy(d => d.Name)
                .ToListAsync();

            var selectedIds = await _db.UserDietaryRestrictions
                .Where(x => x.UserId == userId)
                .Select(x => x.DietaryRestrictionId)
                .ToListAsync();

            var vm = new DietarySettingsViewModel
            {
                Restrictions = allRestrictions.Select(r => new DietaryRestrictionOptionViewModel
                {
                    DietaryRestrictionId = r.Id,
                    Name = r.Name,
                    IsSelected = selectedIds.Contains(r.Id)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dietary(DietarySettingsViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chosen = vm.Restrictions
                .Where(x => x.IsSelected)
                .Select(x => x.DietaryRestrictionId)
                .ToHashSet();

            var existing = await _db.UserDietaryRestrictions
                .Where(x => x.UserId == userId)
                .ToListAsync();

            _db.UserDietaryRestrictions.RemoveRange(existing);

            var newRows = chosen.Select(id => new UserDietaryRestriction
            {
                UserId = userId,
                DietaryRestrictionId = id
            });

            await _db.UserDietaryRestrictions.AddRangeAsync(newRows);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Dietary restrictions saved.";
            return RedirectToAction(nameof(Dietary));
        }
    }
}