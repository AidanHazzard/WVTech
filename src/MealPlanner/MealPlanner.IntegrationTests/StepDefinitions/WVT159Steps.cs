using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT159Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly WebDriverWait _wait;

    public WVT159Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    [Given("'Jack' is on the home page for today")]
    public void GivenJackIsOnTheHomePageForToday()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Home/Index?date={DateTime.Today:yyyy-MM-dd}");
        _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");
    }

    [When("'Jack' deletes a meal from the home page")]
    public void WhenJackDeletesAMealFromTheHomePage()
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function() { return true; };");
        var btn = _wait.Until(d => d.FindElement(By.CssSelector(".mealDeleteButton")));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
    }

    [When("'Jack' checks off a meal on the home page")]
    public void WhenJackChecksOffAMealOnTheHomePage()
    {
        _wait.Until(d => d.FindElement(By.CssSelector(".MealCheckBox"))).Click();
    }

    [Then("'Jack' remains on the home page")]
    public void ThenJackRemainsOnTheHomePage()
    {
        _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        Assert.That(_driver.Url, Does.Contain("Home/Index").Or.StartWith($"{_baseUrl}/").And.Not.Contain("Meal/PlannerHome"));
    }
}
