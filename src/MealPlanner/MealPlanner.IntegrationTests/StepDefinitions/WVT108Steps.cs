using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT108Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT108Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("'Jack' is on the planner page")]
    public void GivenJackIsOnThePlannerPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/PlannerHome");
    }

    [When("'Jack' clicks the delete meal button")]
    public void WhenJackClicksTheDeleteMealButton()
    {
        var deleteButton = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector(".btn-delete-meal")));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", deleteButton);
        deleteButton.Click();
    }

    [When("'Jack' confirms meal deletion")]
    public void WhenJackConfirmsMealDeletion()
    {
        var confirmBtn = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector(".inline-confirm-yes")));
        confirmBtn.Click();
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("a delete confirmation alert is shown")]
    public void ThenADeleteConfirmationAlertIsShown()
    {
        var confirm = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d =>
            {
                var el = d.FindElements(By.CssSelector(".inline-confirm")).FirstOrDefault();
                return el != null && el.Displayed ? el : null;
            });
        Assert.That(confirm, Is.Not.Null, "Expected inline confirmation dialog to be visible");
        Assert.That(_driver.Url, Does.Contain("PlannerHome"));
    }

    [Then("the meal is removed from the planner page")]
    public void ThenTheMealIsRemovedFromThePlannerPage()
    {
        Assert.That(_driver.PageSource, Does.Not.Contain("testMeal"));
    }

    [Given("'Jack' has a weekly repeating meal")]
    public void GivenJackHasAWeeklyRepeatingMeal()
    {
        using var context = BDDSetup.CreateContext();

        var user = context.Users.First(u => u.Email == "Jack@fakeemail.com");

        var meal = new MealPlanner.Models.Meal
        {
            Title = "Weekly Meal",
            UserId = user.Id,
            StartTime = DateTime.Today.AddHours(12),
            RepeatRule = "Weekly"
        };

        context.Meals.Add(meal);
        context.SaveChanges();
    }

    [Then("future repeated meals still exist")]
    public void ThenFutureRepeatedMealsStillExist()
    {
        Assert.That(_driver.PageSource, Does.Contain("Weekly Meal"));
    }
}