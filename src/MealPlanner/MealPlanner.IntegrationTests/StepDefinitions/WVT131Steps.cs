using MealPlanner.Helpers;
using MealPlanner.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT131Steps
{
    private IWebDriver _driver = null!;
    private string _baseUrl = null!;
    private WebDriverWait _wait = null!;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    [Given("'Gary' is on the create meal page")]
    public void GivenGaryIsOnTheCreateMealPage()
    {
        try { _wait.Until(d => !d.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase)); }
        catch (WebDriverTimeoutException) { }
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
        WaitForPageLoad();
    }

    [Given("'Gary' opens the edit meal page for {string}")]
    [When("'Gary' opens the edit meal page for {string}")]
    public void GivenGaryOpensEditMealPageFor(string mealTitle)
    {
        using var ctx = BDDSetup.CreateContext();
        var meal = ctx.Meals.First(m => m.Title == mealTitle);
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/EditMeal?id={meal.Id}");
        WaitForPageLoad();
    }

    // ── Form interactions ────────────────────────────────────────────────────

    [Given("'Gary' enters the meal title {string}")]
    public void GivenGaryEntersTheMealTitle(string title)
    {
        var input = _wait.Until(d => d.FindElement(By.Id("Title")));
        input.Clear();
        input.SendKeys(title);
    }

    [Given("'Gary' sets the meal date to next Monday")]
    public void GivenGarySetsMealDateToNextMonday()
    {
        var nextMonday = GetNextOccurrence(DayOfWeek.Monday);
        SetSelect("SelectedMonth", nextMonday.Month.ToString());
        SetSelect("SelectedDay", nextMonday.Day.ToString());
    }

    [Given("'Gary' enables weekly repeat")]
    [When("'Gary' enables weekly repeat")]
    public void GivenGaryEnablesWeeklyRepeat()
    {
        var checkbox = _wait.Until(d => d.FindElement(By.Id("repeatWeeklyToggle")));
        if (!checkbox.Selected) checkbox.Click();
        _wait.Until(d => d.FindElement(By.Id("repeatDaysPanel")).Displayed);
    }

    [Given("'Gary' selects Monday, Tuesday, and Thursday as repeat days")]
    public void GivenGarySelectsMonTueThu()
    {
        CheckDayCheckbox(DayOfWeek.Monday);
        CheckDayCheckbox(DayOfWeek.Tuesday);
        CheckDayCheckbox(DayOfWeek.Thursday);
    }

    [Given("'Gary' selects Monday and Wednesday as repeat days")]
    public void GivenGarySelectsMonWed()
    {
        CheckDayCheckbox(DayOfWeek.Monday);
        CheckDayCheckbox(DayOfWeek.Wednesday);
    }

    [Given("'Gary' selects {word} as a repeat day")]
    [When("'Gary' selects {word} as a repeat day")]
    public void WhenGarySelectsRepeatDay(string dayName)
    {
        CheckDayCheckbox(Enum.Parse<DayOfWeek>(dayName));
    }

    [When("'Gary' deselects {word} from the repeat days")]
    public void WhenGaryDeselectsRepeatDay(string dayName)
    {
        var day = Enum.Parse<DayOfWeek>(dayName);
        var checkbox = _wait.Until(d =>
        {
            try { var el = d.FindElement(By.Id($"repeatDay_{day}")); return el.Displayed ? el : null; }
            catch (NoSuchElementException) { return null; }
        })!;
        if (checkbox.Selected) checkbox.Click();
    }

    [When("'Gary' creates the meal")]
    public void WhenGaryCreatesTheMeal()
    {
        var btn = _driver.FindElement(By.Id("createMeal"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        WaitForPageLoad();
    }

    [When("'Gary' saves the meal changes")]
    public void WhenGarySavesMealChanges()
    {
        var btn = _driver.FindElement(By.Id("saveMealBtn"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        WaitForPageLoad();
    }

    // ── Assertions ───────────────────────────────────────────────────────────

    [Then("day-of-week checkboxes are visible on the page")]
    public void ThenDayOfWeekCheckboxesAreVisible()
    {
        var checkboxes = _wait.Until(d =>
        {
            var els = d.FindElements(By.CssSelector(".repeat-day-checkbox"));
            return els.Count == 7 && els.All(e => e.Displayed) ? els : null;
        });
        Assert.That(checkboxes, Is.Not.Null);
        Assert.That(checkboxes!.Count, Is.EqualTo(7));
    }

    [Then("the meal {string} appears on the planner on {word}")]
    public void ThenMealAppearsOnPlannerOn(string mealTitle, string dayName)
    {
        NavigateToPlanner(GetNextOccurrence(Enum.Parse<DayOfWeek>(dayName)));
        _wait.Until(d => d.FindElements(By.CssSelector(".list-group-item"))
            .Any(el => el.Text.Contains(mealTitle)));
    }

    [Then("the meal {string} does not appear on the planner on {word}")]
    public void ThenMealDoesNotAppearOnPlannerOn(string mealTitle, string dayName)
    {
        NavigateToPlanner(GetNextOccurrence(Enum.Parse<DayOfWeek>(dayName)));
        WaitForPageLoad();
        var items = _driver.FindElements(By.CssSelector(".list-group-item"));
        Assert.That(items.Any(el => el.Text.Contains(mealTitle)), Is.False,
            $"Expected '{mealTitle}' to NOT appear on planner for {dayName}");
    }

    [Then("the {word} repeat day checkbox is checked")]
    public void ThenRepeatDayCheckboxIsChecked(string dayName)
    {
        var day = Enum.Parse<DayOfWeek>(dayName);
        var checkbox = _wait.Until(d => d.FindElement(By.Id($"repeatDay_{day}")));
        Assert.That(checkbox.Selected, Is.True, $"Expected {dayName} checkbox to be checked");
    }

    [Then("the {word} repeat day checkbox is not checked")]
    public void ThenRepeatDayCheckboxIsNotChecked(string dayName)
    {
        var day = Enum.Parse<DayOfWeek>(dayName);
        var checkbox = _wait.Until(d => d.FindElement(By.Id($"repeatDay_{day}")));
        Assert.That(checkbox.Selected, Is.False, $"Expected {dayName} checkbox to not be checked");
    }

    // ── DB setup ─────────────────────────────────────────────────────────────

    [Given("'Gary' has a weekly repeating meal titled {string} scheduled on {word} and {word}")]
    public void GivenGaryHasWeeklyMealScheduledOnTwoDays(string mealTitle, string day1Name, string day2Name)
    {
        var day1 = Enum.Parse<DayOfWeek>(day1Name);
        var day2 = Enum.Parse<DayOfWeek>(day2Name);

        using var ctx = BDDSetup.CreateContext();
        var user = ctx.Users.First(u => u.NormalizedEmail == "GARY@FAKEEMAIL.COM");

        var existing = ctx.Meals.Where(m => m.UserId == user.Id && m.Title == mealTitle).ToList();
        if (existing.Count > 0) { ctx.Meals.RemoveRange(existing); ctx.SaveChanges(); }

        ctx.Meals.Add(new Meal
        {
            UserId = user.Id,
            Title = mealTitle,
            StartTime = GetNextOccurrence(day1),
            RepeatRule = "Weekly",
            RepeatDays = MealSchedule.EncodeRepeatDays(new[] { day1, day2 })
        });
        ctx.SaveChanges();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void WaitForPageLoad() =>
        _wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");

    private void SetSelect(string selectId, string value)
    {
        var select = _driver.FindElement(By.Id(selectId));
        new SelectElement(select).SelectByValue(value);
    }

    private void CheckDayCheckbox(DayOfWeek day)
    {
        var checkbox = _wait.Until(d =>
        {
            try { var el = d.FindElement(By.Id($"repeatDay_{day}")); return el.Displayed ? el : null; }
            catch (NoSuchElementException) { return null; }
        })!;
        if (!checkbox.Selected) checkbox.Click();
    }

    private void NavigateToPlanner(DateTime date)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/PlannerHome?date={date:yyyy-MM-dd}");
        WaitForPageLoad();
    }

    private static DateTime GetNextOccurrence(DayOfWeek target)
    {
        var today = DateTime.Today;
        int daysUntil = ((int)target - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return today.AddDays(daysUntil);
    }
}
