using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT156Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly WebDriverWait _wait;
    private DateTime _futureDate;

    public WVT156Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    [Given("'Jack' is on the home page for a future date")]
    public void GivenJackIsOnTheHomePageForAFutureDate()
    {
        _futureDate = DateTime.Today.AddDays(7);
        _driver.Navigate().GoToUrl($"{_baseUrl}/Home/Index?date={_futureDate:yyyy-MM-dd}");
        _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("'Jack' clicks Add A Meal")]
    public void WhenJackClicksAddAMeal()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal?date={_futureDate:yyyy-MM-dd}");
        _wait.Until(d => d.Url.Contains("NewMeal"));
    }

    [Then("the create meal form is pre-filled with that date")]
    public void ThenTheCreateMealFormIsPreFilledWithThatDate()
    {
        var monthSelect = new SelectElement(_driver.FindElement(By.Name("SelectedMonth")));
        var daySelect = new SelectElement(_driver.FindElement(By.Name("SelectedDay")));

        Assert.That(monthSelect.SelectedOption.GetAttribute("value"), Is.EqualTo(_futureDate.Month.ToString()));
        Assert.That(daySelect.SelectedOption.GetAttribute("value"), Is.EqualTo(_futureDate.Day.ToString()));
    }

    [When("'Jack' creates the meal with title 'Future Meal'")]
    public void WhenJackCreatesTheMealWithTitleFutureMeal()
    {
        _wait.Until(d => d.FindElement(By.CssSelector("#createMealForm input[name='Title']"))).SendKeys("Future Meal");
        _wait.Until(d => d.FindElement(By.Id("createMealForm"))).Submit();
        _wait.Until(d => !d.Url.Contains("NewMeal"));
    }

    [Then("the meal 'Future Meal' is saved on that future date")]
    public void ThenTheMealFutureMealIsSavedOnThatFutureDate()
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = ctx.Users.First(u => u.NormalizedEmail == "JACK@FAKEEMAIL.COM").Id;
        var meal = ctx.Meals.FirstOrDefault(m => m.UserId == userId && m.Title == "Future Meal");

        Assert.That(meal, Is.Not.Null);
        Assert.That(meal!.StartTime!.Value.Date, Is.EqualTo(_futureDate.Date));
    }
}
