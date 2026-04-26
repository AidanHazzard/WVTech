using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Services;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Controllers
{
    // TODO: Remove business logic from controller
    [Authorize]
    public class UserSettingsController : Controller
    {
        private readonly MealPlannerDBContext _db;
        private readonly IUserSettingsRepository _userSettings;
        private readonly IUserSettingsService _userSettingsService;
        private readonly ITagRepository _tagRepository;
        private readonly IUserFoodPreferenceRepository _foodPrefRepository;

        public UserSettingsController(MealPlannerDBContext db, IUserSettingsRepository userSettings, IUserSettingsService userSettingsService, ITagRepository tagRepository, IUserFoodPreferenceRepository foodPrefRepository)
        {
            _db = db;
            _userSettings = userSettings;
            _userSettingsService = userSettingsService;
            _tagRepository = tagRepository;
            _foodPrefRepository = foodPrefRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var vm = new FoodPreferenceViewModel
            {
                CurrentPreferences = await _foodPrefRepository.GetFoodPreferenceNamesAsync(userId),
                AvailableTags = await _tagRepository.GetTagNamesAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFoodPreferences(FoodPreferenceViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            if (vm.NewPreferences.Count > 0)
                await _foodPrefRepository.AddFoodPreferencesAsync(userId, vm.NewPreferences);

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFoodPreference(string tagName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await _foodPrefRepository.RemoveFoodPreferenceAsync(userId, tagName);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ThemeChange()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _userSettings.ToggleDarkThemeAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleTheme()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            await _userSettings.ToggleDarkThemeAsync(userId);
            return Ok();
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

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new UserSettingsResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            UserSettingsResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userSettingsService.ResetPasswordAsync(
                User,
                model.Password,
                model.NewPassword);

            if (result.Succeeded)
                return ResetPasswordSuccess();

            AddErrors(result);
            return View(model);
        }

        // --- private helpers (controller-only concerns) ---

        private IActionResult ResetPasswordSuccess()
        {
            ViewBag.StatusMessage = "Your password has been reset successfully.";
            ModelState.Clear();
            return View(new UserSettingsResetPasswordViewModel());
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}