using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT141Steps
    {
        private readonly SharedDriver _shared;
        private readonly ScenarioContext _scenarioContext;

        public WVT141Steps(SharedDriver shared, ScenarioContext scenarioContext)
        {
            _shared = shared;
            _scenarioContext = scenarioContext;
        }

        [Then("the meal title is shown on the page")]
        public void ThenTheMealTitleIsShownOnThePage()
        {
            var title = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#mealTitle"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            Assert.That(title.Text, Is.EqualTo("Test Meal With Recipes"));
        }
    }
}