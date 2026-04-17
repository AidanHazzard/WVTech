using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT143Steps
    {
        private string _clickedRecipeId = null!;
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;

        // Runs before each scenerio
        [BeforeScenario]
        public void SetUp()
        {
            _driver = BDDSetup.Driver;
            _wait = BDDSetup.Wait;
        }

        [When("'Jack' clicks on their recipe")]
        public void WhenJackClicksOnTheirRecipe()
        {
            var firstItem = _wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector(".mealRecipeItem"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            _clickedRecipeId = firstItem.GetAttribute("data-recipe-id");
            firstItem.Click();
        }

        [Then("'Jack' is taken to the recipe detail page")]
        public void ThenJackIsTakenToTheRecipeDetailPage()
        {
            _wait.Until(driver =>
                driver.Url.Contains($"/FoodEntries/Recipes/{_clickedRecipeId}"));

            Assert.That(_driver.Url, Does.Contain($"/FoodEntries/Recipes/{_clickedRecipeId}"));
        }
    }
}