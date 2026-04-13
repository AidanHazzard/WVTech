using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using Mealplanner.IntegrationTests;
using MealPlanner.IntegrationTests;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT118Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT118Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [When("'Jack' navigates to the nutrition page")]
    public void WhenJackNavigatesToTheNutritionPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Nutrition");
    }

    [Then("the calories nutrition bar is shown")]
    public void ThenTheCaloriesNutritionBarIsShown()
    {
        var bar = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("caloriesBar")));
        Assert.That(bar.Displayed, Is.True);
    }

    [Then("the protein nutrition bar is shown")]
    public void ThenTheProteinNutritionBarIsShown()
    {
        var bar = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("proteinBar")));
        Assert.That(bar.Displayed, Is.True);
    }

    [Then("the fat nutrition bar is shown")]
    public void ThenTheFatNutritionBarIsShown()
    {
        var bar = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("fatBar")));
        Assert.That(bar.Displayed, Is.True);
    }

    [Then("the carbs nutrition bar is shown")]
    public void ThenTheCarbsNutritionBarIsShown()
    {
        var bar = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("carbsBar")));
        Assert.That(bar.Displayed, Is.True);
    }

    [Then("the calories nutrition fraction is shown")]
    public void ThenTheCaloriesNutritionFractionIsShown()
    {
        var fraction = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("caloriesFraction")));
        Assert.That(fraction.Text.Trim(), Is.Not.Empty);
    }

    [Then("the protein nutrition fraction is shown")]
    public void ThenTheProteinNutritionFractionIsShown()
    {
        var fraction = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("proteinFraction")));
        Assert.That(fraction.Text.Trim(), Is.Not.Empty);
    }

    [Then("the fat nutrition fraction is shown")]
    public void ThenTheFatNutritionFractionIsShown()
    {
        var fraction = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("fatFraction")));
        Assert.That(fraction.Text.Trim(), Is.Not.Empty);
    }

    [Then("the carbs nutrition fraction is shown")]
    public void ThenTheCarbsNutritionFractionIsShown()
    {
        var fraction = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("carbsFraction")));
        Assert.That(fraction.Text.Trim(), Is.Not.Empty);
    }
}