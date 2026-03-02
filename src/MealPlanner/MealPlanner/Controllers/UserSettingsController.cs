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
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

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
                    Name = r.Name ?? "",
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
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

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

        [HttpGet]
        public async Task<IActionResult> Nutrition()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var pref = await _db.UserNutritionPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId);

            var vm = new NutritionSettingsViewModel
            {
                CalorieTarget = pref?.CalorieTarget,
                ProteinTarget = pref?.ProteinTarget,
                CarbTarget = pref?.CarbTarget,
                FatTarget = pref?.FatTarget
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nutrition(NutritionSettingsViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var pref = await _db.UserNutritionPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (pref == null)
            {
                pref = new UserNutritionPreference
                {
                    UserId = userId
                };
                _db.UserNutritionPreferences.Add(pref);
            }

            pref.CalorieTarget = vm.CalorieTarget;
            pref.ProteinTarget = vm.ProteinTarget;
            pref.CarbTarget = vm.CarbTarget;
            pref.FatTarget = vm.FatTarget;

            await _db.SaveChangesAsync();

            TempData["Message"] = "Nutrition goals saved.";
            return RedirectToAction(nameof(Nutrition));
        }
    }
}