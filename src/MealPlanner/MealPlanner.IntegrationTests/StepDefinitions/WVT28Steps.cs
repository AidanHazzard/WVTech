using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT28Steps
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly string _baseUrl;

    public WVT28Steps()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} has a meal named {string} today with a recipe containing ingredient {string}")]
    public void GivenUserHasMealTodayWithRecipeContainingIngredient(string userName, string mealTitle, string ingredientName)
    {
        var email = $"{userName}@fakeemail.com";
        using var ctx = BDDSetup.CreateContext();

        var user = ctx.Set<User>().FirstOrDefault(u => u.Email == email);
        Assert.That(user, Is.Not.Null, $"User '{userName}' not found");

        var normalizedName = IngredientNameNormalizer.NormalizeKey(ingredientName);
        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(b => b.Name == normalizedName)
            ?? ctx.Set<IngredientBase>().Add(new IngredientBase { Name = normalizedName }).Entity;

        var measurement = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == "Count")
            ?? ctx.Set<Measurement>().Add(new Measurement { Name = "Count" }).Entity;

        ctx.SaveChanges();

        var ingredient = new Ingredient
        {
            DisplayName = ingredientName,
            Amount = 1,
            IngredientBase = ingredientBase,
            Measurement = measurement
        };

        var recipe = new Recipe
        {
            Name = $"{mealTitle} Recipe",
            Directions = "Test",
            Calories = 0
        };
        recipe.Ingredients.Add(ingredient);

        var meal = new Meal
        {
            UserId = user!.Id,
            Title = mealTitle,
            StartTime = DateTime.Today.AddHours(12)
        };
        meal.Recipes.Add(recipe);

        ctx.Recipes.Add(recipe);
        ctx.Meals.Add(meal);
        ctx.SaveChanges();
    }

    [Given("{string} has a meal named {string} today with a recipe containing {string} {string} of {string}")]
    public void GivenUserHasMealTodayWithRecipeContainingAmountAndMeasurementOfIngredient(
        string userName, string mealTitle, string amount, string measurement, string ingredientName)
    {
        var email = $"{userName}@fakeemail.com";
        using var ctx = BDDSetup.CreateContext();

        var user = ctx.Set<User>().FirstOrDefault(u => u.Email == email);
        Assert.That(user, Is.Not.Null, $"User '{userName}' not found");

        var normalizedName = IngredientNameNormalizer.NormalizeKey(ingredientName);
        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(b => b.Name == normalizedName)
            ?? ctx.Set<IngredientBase>().Add(new IngredientBase { Name = normalizedName }).Entity;

        var measurementEntity = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == measurement)
            ?? ctx.Set<Measurement>().Add(new Measurement { Name = measurement }).Entity;

        ctx.SaveChanges();

        var ingredient = new Ingredient
        {
            DisplayName = ingredientName,
            Amount = float.Parse(amount),
            IngredientBase = ingredientBase,
            Measurement = measurementEntity
        };

        var recipe = new Recipe
        {
            Name = $"{mealTitle} Recipe",
            Directions = "Test",
            Calories = 0
        };
        recipe.Ingredients.Add(ingredient);

        var meal = new Meal
        {
            UserId = user!.Id,
            Title = mealTitle,
            StartTime = DateTime.Today.AddHours(12)
        };
        meal.Recipes.Add(recipe);

        ctx.Recipes.Add(recipe);
        ctx.Meals.Add(meal);
        ctx.SaveChanges();
    }

    [Given("{string} has completed meal {string} with auto-removed pantry ingredient {string}")]
    public void GivenUserHasCompletedMealWithAutoRemovedIngredient(string userName, string mealTitle, string ingredientName)
    {
        var email = $"{userName}@fakeemail.com";
        using var ctx = BDDSetup.CreateContext();

        var user = ctx.Set<User>().FirstOrDefault(u => u.Email == email);
        Assert.That(user, Is.Not.Null, $"User '{userName}' not found");

        var meal = ctx.Meals
            .Include(m => m.Recipes)
            .FirstOrDefault(m => m.UserId == user!.Id && m.Title == mealTitle);
        Assert.That(meal, Is.Not.Null, $"Meal '{mealTitle}' not found for user '{userName}'");

        var completionDate = DateTime.Today;
        var existing = ctx.MealCompletions.Find(meal!.Id, completionDate);
        if (existing == null)
        {
            ctx.MealCompletions.Add(new MealCompletion { MealId = meal.Id, CompletionDate = completionDate });
        }

        var normalizedName = IngredientNameNormalizer.NormalizeKey(ingredientName);
        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(b => b.Name == normalizedName);
        Assert.That(ingredientBase, Is.Not.Null, $"IngredientBase '{ingredientName}' not found");

        var measurement = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == "Count")
            ?? ctx.Set<Measurement>().Add(new Measurement { Name = "Count" }).Entity;

        ctx.SaveChanges();

        var alreadyTracked = ctx.MealAutoRemovedIngredients
            .Any(r => r.MealId == meal.Id && r.CompletionDate == completionDate && r.IngredientBaseId == ingredientBase!.Id);

        if (!alreadyTracked)
        {
            ctx.MealAutoRemovedIngredients.Add(new MealAutoRemovedIngredient
            {
                MealId = meal.Id,
                CompletionDate = completionDate,
                IngredientBaseId = ingredientBase!.Id,
                DisplayName = ingredientName,
                Amount = 2,
                MeasurementId = measurement.Id,
                CreatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();
        }
    }

    [Given("{string} has no pantry items")]
    public void GivenUserHasNoPantryItems(string userName)
    {
        var email = $"{userName}@fakeemail.com";
        using var ctx = BDDSetup.CreateContext();
        var user = ctx.Set<User>().Include(u => u.PantryItems).FirstOrDefault(u => u.Email == email);
        Assert.That(user, Is.Not.Null, $"User '{userName}' not found");
        user!.PantryItems.Clear();
        ctx.SaveChanges();
    }

    [Given("{string} has a pantry item named {string} with amount {string} and measurement {string}")]
    public void GivenUserHasPantryItem(string userName, string ingredientName, string amount, string measurement)
    {
        var email = $"{userName}@fakeemail.com";
        using var ctx = BDDSetup.CreateContext();

        var user = ctx.Set<User>().Include(u => u.PantryItems).FirstOrDefault(u => u.Email == email);
        Assert.That(user, Is.Not.Null, $"User '{userName}' not found");

        var normalizedName = IngredientNameNormalizer.NormalizeKey(ingredientName);
        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(b => b.Name == normalizedName)
            ?? ctx.Set<IngredientBase>().Add(new IngredientBase { Name = normalizedName }).Entity;

        var measurementEntity = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == measurement)
            ?? ctx.Set<Measurement>().Add(new Measurement { Name = measurement }).Entity;

        ctx.SaveChanges();

        user!.PantryItems.Add(new Ingredient
        {
            DisplayName = ingredientName,
            Amount = float.Parse(amount),
            IngredientBase = ingredientBase,
            Measurement = measurementEntity
        });
        ctx.SaveChanges();
    }

    [Given("{string} is on the pantry page")]
    [When("{string} is on the pantry page")]
    public void GivenUserIsOnPantryPage(string userName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Shopping/Pantry");
        WaitForPageLoad();
    }

    [Then("{string} appears in the pantry list")]
    public void ThenIngredientAppearsInPantryList(string ingredientName)
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item-name"));
        Assert.That(items.Any(i => i.Text.Contains(ingredientName, StringComparison.OrdinalIgnoreCase)),
            Is.True, $"Expected '{ingredientName}' to appear in the pantry list but it did not.");
    }

    [Then("{string} does not appear in the pantry list")]
    public void ThenIngredientDoesNotAppearInPantryList(string ingredientName)
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item-name"));
        Assert.That(items.Any(i => i.Text.Contains(ingredientName, StringComparison.OrdinalIgnoreCase)),
            Is.False, $"Expected '{ingredientName}' to not appear in the pantry list but it did.");
    }

    [When("{string} marks the meal {string} as completed")]
    public void WhenUserMarksNamedMealAsCompleted(string userName, string mealTitle)
    {
        NavigateToHomeForToday();
        ClickMealCheckbox(mealTitle, expectChecked: false);
    }

    [When("{string} marks the meal {string} as incomplete")]
    public void WhenUserMarksNamedMealAsIncomplete(string userName, string mealTitle)
    {
        NavigateToHomeForToday();
        ClickMealCheckbox(mealTitle, expectChecked: true);
    }

    [Then("a pantry removal prompt is displayed")]
    public void ThenPantryRemovalPromptIsDisplayed()
    {
        AssertModalVisible("Would you like to remove");
    }

    [Then("a pantry restore prompt is displayed")]
    public void ThenPantryRestorePromptIsDisplayed()
    {
        AssertModalVisible("Would you like to add");
    }

    [When("{string} accepts the pantry removal prompt")]
    public void WhenUserAcceptsPantryRemovalPrompt(string userName)
    {
        ClickModalButton("pantryPromptAccept");
        WaitForPageLoad();
    }

    [When("{string} declines the pantry removal prompt")]
    public void WhenUserDeclinesPantryRemovalPrompt(string userName)
    {
        ClickModalButton("pantryPromptDecline");
        WaitForPageLoad();
    }

    [When("{string} accepts the pantry restore prompt")]
    public void WhenUserAcceptsPantryRestorePrompt(string userName)
    {
        ClickModalButton("pantryPromptAccept");
        WaitForPageLoad();
    }

    [When("{string} declines the pantry restore prompt")]
    public void WhenUserDeclinesPantryRestorePrompt(string userName)
    {
        ClickModalButton("pantryPromptDecline");
        WaitForPageLoad();
    }

    private void NavigateToHomeForToday()
    {
        var url = $"{_baseUrl}/Home/Index?date={DateTime.Today:yyyy-MM-dd}";
        _driver.Navigate().GoToUrl(url);
        WaitForPageLoad();
    }

    private void ClickMealCheckbox(string mealTitle, bool expectChecked)
    {
        IWebElement? checkbox = null;
        _wait.Until(d =>
        {
            try
            {
                var checkboxes = d.FindElements(By.CssSelector(".MealCheckBox"));
                foreach (var cb in checkboxes)
                {
                    var form = cb.FindElement(By.XPath("ancestor::form[contains(@class,'mealCompleteForm')]"));
                    var mealCard = form.FindElement(By.XPath("following-sibling::form[contains(@class,'mealViewForm')]//h2"));
                    if (mealCard.Text.Contains(mealTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        checkbox = cb;
                        return true;
                    }
                }
                return false;
            }
            catch (StaleElementReferenceException) { return false; }
            catch (NoSuchElementException) { return false; }
        });

        Assert.That(checkbox, Is.Not.Null, $"Checkbox for meal '{mealTitle}' not found");
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", checkbox!);
        checkbox!.Click();
    }

    private void AssertModalVisible(string expectedMessageFragment)
    {
        _wait.Until(d =>
        {
            try
            {
                var modal = d.FindElement(By.Id("pantryPromptModal"));
                return modal.Displayed && modal.GetCssValue("display") != "none";
            }
            catch (NoSuchElementException) { return false; }
            catch (StaleElementReferenceException) { return false; }
        });

        var message = _driver.FindElement(By.Id("pantryPromptMessage")).Text;
        Assert.That(message, Does.Contain(expectedMessageFragment).IgnoreCase);
    }

    private void ClickModalButton(string buttonId)
    {
        var btn = _wait.Until(d =>
        {
            try
            {
                var el = d.FindElement(By.Id(buttonId));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(btn, Is.Not.Null, $"Modal button '{buttonId}' not found");
        btn!.Click();
    }

    private void WaitForPageLoad()
    {
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }
}
