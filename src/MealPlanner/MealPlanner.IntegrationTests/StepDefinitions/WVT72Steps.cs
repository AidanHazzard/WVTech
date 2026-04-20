using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT72Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT72Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [When("'Jack' selects meal date {string}")]
    public void WhenJackSelectsMealDate(string date)
    {
        var dateInput = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("Date")));
        dateInput.Clear();
        dateInput.SendKeys(date);
    }

    [When("'Jack' enables weekly repeat")]
    public void WhenJackEnablesWeeklyRepeat()
    {
        var repeatCheckbox = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("RepeatWeekly")));

        if (!repeatCheckbox.Selected)
            repeatCheckbox.Click();
    }

    [Then("the meal date field is saved as {string}")]
    public void ThenTheMealDateFieldIsSavedAs(string expectedDate)
    {
        var dateInput = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("Date")));

        Assert.That(dateInput.GetAttribute("value"), Does.Contain(expectedDate));
    }

    [Then("the meal repeat rule is saved as weekly")]
    public void ThenTheMealRepeatRuleIsSavedAsWeekly()
    {
        var repeatCheckbox = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("RepeatWeekly")));

        Assert.That(repeatCheckbox.Selected, Is.True);
    }
}