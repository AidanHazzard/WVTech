using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using Mealplanner.IntegrationTests;
using MealPlanner.IntegrationTests;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT120Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT120Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Then("the planner shows the meal title {string}")]
    public void ThenThePlannerShowsTheMealTitle(string mealTitle)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/PlannerHome");

        var mealTitleElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.XPath($"//*[contains(text(), '{mealTitle}')]")));

        Assert.That(mealTitleElement.Displayed, Is.True);
    }

    [Then("the new meal page does not require a time field")]
    public void ThenTheNewMealPageDoesNotRequireATimeField()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");

        var timeFields = _driver.FindElements(By.Name("Time"));
        Assert.That(timeFields.Count, Is.EqualTo(0));
    }
}