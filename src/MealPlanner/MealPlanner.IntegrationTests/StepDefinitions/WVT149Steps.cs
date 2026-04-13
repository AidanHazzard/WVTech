using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT149Steps
    {
        private readonly SharedDriver _shared;

        public WVT149Steps(SharedDriver shared)
        {
            _shared = shared;
        }

        [Given("'Jack' is on the user settings page")]
        public void GivenJackIsOnTheUserSettingsPage()
        {
            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/UserSettings/Index");
            _shared.Wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [Then("a theme toggle is shown on the settings page")]
        public void ThenAThemeToggleIsShownOnTheSettingsPage()
        {
            var toggle = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            Assert.That(toggle, Is.Not.Null);
        }

        [When("'Jack' clicks the theme toggle")]
        public void WhenJackClicksTheThemeToggle()
        {
            var toggle = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            toggle.Click();
        }

       [Then("the theme changes")]
public void ThenTheThemeChanges()
{
    var js = (IJavaScriptExecutor)_shared.Driver;
    var toggle = _shared.Driver.FindElement(By.CssSelector("#themeToggle"));
    var isChecked = js.ExecuteScript("return document.getElementById('themeToggle').checked;") as bool? == true;
    var theme = js.ExecuteScript("return document.documentElement.getAttribute('data-theme');")?.ToString();

    if (isChecked)
        Assert.That(theme, Is.EqualTo("light"));
    else
        Assert.That(theme, Is.Null.Or.Empty);
}
    }
}