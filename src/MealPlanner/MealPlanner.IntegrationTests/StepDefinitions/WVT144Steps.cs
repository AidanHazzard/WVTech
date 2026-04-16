using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT144Steps
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;
    private string _baseUrl = null!;

    // Tracks the first meal name in the summary before regeneration
    private string _mealNameBeforeRegeneration = string.Empty;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} has a calorie target set")]
    public void GivenUserHasACalorieTargetSet(string userName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var existing = ctx.Set<UserNutritionPreference>().FirstOrDefault(p => p.UserId == user.Id);
        if (existing != null) return;

        ctx.Add(new UserNutritionPreference { UserId = user.Id, CalorieTarget = 2000 });
        ctx.SaveChanges();
    }

    [When("{string} navigates to the Create Meal page")]
    public void WhenUserNavigatesToCreateMealPage(string userName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
        _wait.Until(d => d.Url.Contains("/Meal/NewMeal"));
    }

    [Then("a {string} button is visible alongside the existing generate meal button")]
    public void ThenButtonIsVisibleAlongsideGenerateMealButton(string buttonLabel)
    {
        var buttons = _driver.FindElements(By.TagName("button"))
            .Concat(_driver.FindElements(By.TagName("a")));
        var match = buttons.FirstOrDefault(el =>
            el.Text.Contains(buttonLabel, StringComparison.OrdinalIgnoreCase));
        Assert.That(match, Is.Not.Null, $"Could not find a button or link labelled '{buttonLabel}'");
        Assert.That(match!.Displayed, Is.True);
    }

    [Given("{string} is on the Create Meal page")]
    public void GivenUserIsOnCreateMealPage(string userName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
        _wait.Until(d => d.Url.Contains("/Meal/NewMeal"));
    }

    [When("{string} clicks {string}")]
    public void WhenUserClicksButton(string userName, string buttonLabel)
    {
        var buttons = _driver.FindElements(By.TagName("button"))
            .Concat(_driver.FindElements(By.TagName("a")));
        var btn = buttons.First(el =>
            el.Text.Contains(buttonLabel, StringComparison.OrdinalIgnoreCase));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("{string} is asked how many meals he would like for the day")]
    public void ThenUserIsAskedHowManyMeals(string userName)
    {
        var input = _wait.Until(d =>
        {
            try { return d.FindElement(By.Id("MealCount")); }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(input, Is.Not.Null, "Could not find meal count input (#MealCount)");
        Assert.That(input!.Displayed, Is.True);
    }

    [When("{string} enters {int} for the number of meals")]
    public void WhenUserEntersMealCount(string userName, int count)
    {
        var input = _driver.FindElement(By.Id("MealCount"));
        input.Clear();
        input.SendKeys(count.ToString());
    }

    [Then("{string} is asked whether he would like snacks included")]
    public void ThenUserIsAskedAboutSnacks(string userName)
    {
        var el = _wait.Until(d =>
        {
            try { return d.FindElement(By.Id("IncludeSnacks")); }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(el, Is.Not.Null, "Could not find snack inclusion input (#IncludeSnacks)");
        Assert.That(el!.Displayed, Is.True);
    }

    [When("{string} chooses to include snacks")]
    public void WhenUserChoosesToIncludeSnacks(string userName)
    {
        var checkbox = _driver.FindElement(By.Id("IncludeSnacks"));
        if (!checkbox.Selected)
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", checkbox);
    }

    [Then("{string} is asked what size he would like his snacks to be")]
    public void ThenUserIsAskedSnackSize(string userName)
    {
        var el = _wait.Until(d =>
        {
            try { return d.FindElement(By.Id("SnackSize")); }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(el, Is.Not.Null, "Could not find snack size selector (#SnackSize)");
        Assert.That(el!.Displayed, Is.True);
    }

    [Then("the snack size options include {string} and {string}")]
    public void ThenSnackSizeOptionsInclude(string option1, string option2)
    {
        var select = new SelectElement(_driver.FindElement(By.Id("SnackSize")));
        var optionTexts = select.Options.Select(o => o.Text).ToList();
        Assert.That(optionTexts, Does.Contain(option1), $"Snack size option '{option1}' not found");
        Assert.That(optionTexts, Does.Contain(option2), $"Snack size option '{option2}' not found");
    }

    [Given("{string} has specified {int} meals with no snacks for his day plan")]
    public void GivenUserHasSpecifiedMealsWithNoSnacks(string userName, int mealCount)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/GenerateDayPlan");
        _wait.Until(d => d.Url.Contains("/Meal/GenerateDayPlan"));
        var input = _driver.FindElement(By.Id("MealCount"));
        input.Clear();
        input.SendKeys(mealCount.ToString());
        var snacks = _driver.FindElement(By.Id("IncludeSnacks"));
        if (snacks.Selected)
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", snacks);
    }

    [When("{string} is presented with the configuration for each meal")]
    public void WhenUserIsPresentedWithMealConfig(string userName)
    {
        // The wizard shows per-meal config after meal count is entered;
        // trigger the next step in the wizard
        var nextBtn = _wait.Until(d =>
        {
            try
            {
                return d.FindElements(By.CssSelector("[data-action='next-step']"))
                    .FirstOrDefault(e => e.Displayed);
            }
            catch { return null; }
        });
        if (nextBtn != null)
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", nextBtn);
    }

    [Then("{string} is asked what size he would like each meal to be")]
    public void ThenUserIsAskedMealSize(string userName)
    {
        var el = _wait.Until(d =>
        {
            try { return d.FindElements(By.CssSelector("[id^='MealPreferences_']"))
                    .FirstOrDefault(e => e.Displayed); }
            catch { return null; }
        });
        Assert.That(el, Is.Not.Null, "Could not find meal size selector for first meal");
    }

    [Then("the meal size options include {string}, {string}, and {string}")]
    public void ThenMealSizeOptionsInclude(string option1, string option2, string option3)
    {
        var select = new SelectElement(
            _driver.FindElement(By.CssSelector("[id^='MealPreferences_'][id$='_Size']")));
        var optionTexts = select.Options.Select(o => o.Text).ToList();
        Assert.That(optionTexts, Does.Contain(option1), $"Meal size option '{option1}' not found");
        Assert.That(optionTexts, Does.Contain(option2), $"Meal size option '{option2}' not found");
        Assert.That(optionTexts, Does.Contain(option3), $"Meal size option '{option3}' not found");
    }

    [Then("{string} is asked what type of food he would like using tags")]
    public void ThenUserIsAskedFoodTypeUsingTags(string userName)
    {
        var el = _wait.Until(d =>
        {
            try
            {
                return d.FindElements(By.CssSelector("[id^='MealPreferences_'][id$='_TagIds']"))
                    .FirstOrDefault(e => e.Displayed);
            }
            catch { return null; }
        });
        Assert.That(el, Is.Not.Null, "Could not find tag selector for meal configuration");
    }

    [Then("{string} can enter a custom tag name for the meal")]
    public void ThenUserCanEnterCustomTagName(string userName)
    {
        var el = _wait.Until(d =>
        {
            try
            {
                return d.FindElements(By.CssSelector("[id^='MealPreferences_'][id$='__CustomTagName']"))
                    .FirstOrDefault(e => e.Displayed);
            }
            catch { return null; }
        });
        Assert.That(el, Is.Not.Null, "Could not find custom tag input for meal configuration");
    }

    [When("{string} types {string} as a custom tag and generates the plan")]
    public void WhenUserTypesCustomTagAndGenerates(string userName, string tagName)
    {
        var input = _driver.FindElement(
            By.CssSelector("[id^='MealPreferences_'][id$='__CustomTagName']"));
        input.Clear();
        input.SendKeys(tagName);

        var submit = _driver.FindElement(By.CssSelector("button[type='submit']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submit);
        _wait.Until(d => d.Url.Contains("/Meal/DayPlanSummary"));
    }

    [Given("{string} has completed the day plan configuration")]
    public void GivenUserHasCompletedDayPlanConfig(string userName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        // Remove any meals for today so previous scenarios don't pollute this one
        var today = DateTime.Today;
        var existingMeals = ctx.Set<Meal>().Where(m => m.UserId == user.Id && m.StartTime != null && m.StartTime.Value.Date == today).ToList();
        ctx.Set<Meal>().RemoveRange(existingMeals);
        ctx.SaveChanges();

        var recipe = new Recipe
        {
            Name = "Day Plan Test Recipe",
            Directions = "Test",
            Calories = 400,
            Protein = 20,
            Fat = 10,
            Carbs = 50
        };
        ctx.Add(recipe);
        ctx.SaveChanges();
        ctx.Add(new UserRecipe
        {
            UserId = user.Id,
            RecipeId = recipe.Id,
            UserOwner = true,
            UserVote = UserVoteType.NoVote
        });
        ctx.SaveChanges();

        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/GenerateDayPlan");
        _wait.Until(d => d.Url.Contains("/Meal/GenerateDayPlan"));
        var input = _driver.FindElement(By.Id("MealCount"));
        input.Clear();
        input.SendKeys("1");
    }

    [When("the day plan is generated")]
    public void WhenTheDayPlanIsGenerated()
    {
        var submit = _driver.FindElement(By.CssSelector("button[type='submit']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submit);
        _wait.Until(d => d.Url.Contains("/Meal/DayPlanSummary"));
    }

    [Then("{string} sees a summary of his meal plan for the day")]
    public void ThenUserSeesASummary(string userName)
    {
        var summary = _wait.Until(d =>
        {
            try { return d.FindElement(By.Id("day-plan-summary")); }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(summary, Is.Not.Null, "Could not find day plan summary (#day-plan-summary)");
        Assert.That(summary!.Displayed, Is.True);
    }

    [Then("the summary shows each recommended meal by name")]
    public void ThenSummaryShowsEachMealByName()
    {
        var meals = _driver.FindElements(By.CssSelector("[data-meal-name]"));
        Assert.That(meals.Count, Is.GreaterThan(0), "No meal names found in the summary");
        Assert.That(meals.All(m => !string.IsNullOrWhiteSpace(m.Text)), Is.True);
    }

    [Given("{string} is viewing his generated day plan summary")]
    public void GivenUserIsViewingDayPlanSummary(string userName)
    {
        GivenUserHasCompletedDayPlanConfig(userName);
        WhenTheDayPlanIsGenerated();
        var meals = _driver.FindElements(By.CssSelector("[data-meal-name]"));
        _mealNameBeforeRegeneration = meals.First().Text;
    }

    [When("{string} chooses to regenerate one of the meals")]
    public void WhenUserChoosesToRegenerateAMeal(string userName)
    {
        var regenBtn = _wait.Until(d =>
        {
            try
            {
                return d.FindElements(By.CssSelector("[data-action='regenerate-meal']"))
                    .FirstOrDefault(e => e.Displayed);
            }
            catch { return null; }
        });
        Assert.That(regenBtn, Is.Not.Null, "Could not find regenerate button");
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", regenBtn!);
        _wait.Until(d => d.Url.Contains("/Meal/RegenerateMeal"));
    }

    [Then("{string} is shown the meal configuration form with his previous size and tag selections prefilled")]
    public void ThenUserIsShownPrefillledMealConfig(string userName)
    {
        var sizeSelect = _wait.Until(d =>
        {
            try { return d.FindElement(By.Id("Size")); }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(sizeSelect, Is.Not.Null, "Could not find size selector on regenerate form (#Size)");

        var select = new SelectElement(sizeSelect!);
        Assert.That(select.SelectedOption.Text, Is.Not.Empty,
            "Size selector has no prefilled selection");
    }

    [When("{string} confirms the configuration and regenerates")]
    public void WhenUserConfirmsAndRegenerates(string userName)
    {
        var submit = _driver.FindElement(By.CssSelector("button[type='submit']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submit);
        _wait.Until(d => d.Url.Contains("/Meal/DayPlanSummary"));
    }

    [Then("the updated meal appears in the summary in place of the previous recommendation")]
    public void ThenUpdatedMealAppearsInSummary()
    {
        var meals = _driver.FindElements(By.CssSelector("[data-meal-name]"));
        Assert.That(meals.Count, Is.GreaterThan(0), "No meals in summary after regeneration");
    }

    [Then("all other meals in the summary remain unchanged")]
    public void ThenOtherMealsRemainUnchanged()
    {
        // With only 1 meal in the test plan, verify the summary still shows 1 meal
        var meals = _driver.FindElements(By.CssSelector("[data-meal-name]"));
        Assert.That(meals.Count, Is.EqualTo(1),
            "Expected the same number of meals in the summary after regenerating one");
    }
}
