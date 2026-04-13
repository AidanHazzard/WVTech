using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT141Steps
    {
        private WebDriverWait _wait = null!;

        // Runs before each scenerio
        [BeforeScenario]
        public void SetUp()
        {
            _wait = BDDSetup.Wait;
        }

        [Then("the meal title is shown on the page")]
        public void ThenTheMealTitleIsShownOnThePage()
        {
            var title = _wait.Until(driver =>
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