using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT143Steps
    {
        private readonly SharedDriver _shared;
        private string _clickedRecipeId = null!;

        public WVT143Steps(SharedDriver shared)
        {
            _shared = shared;
        }

        [When("'Jack' clicks on their recipe")]
        public void WhenJackClicksOnTheirRecipe()
        {
            var firstItem = _shared.Wait.Until(driver =>
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
            _shared.Wait.Until(driver =>
                driver.Url.Contains($"/FoodEntries/Recipes/{_clickedRecipeId}"));

            Assert.That(_shared.Driver.Url, Does.Contain($"/FoodEntries/Recipes/{_clickedRecipeId}"));
        }
    }
}