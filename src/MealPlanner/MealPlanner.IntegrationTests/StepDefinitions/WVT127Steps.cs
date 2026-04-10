using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class WVT127Steps
{
    private readonly SharedDriver _shared;
    private IWebElement _titleInput = null!;
    private int _mealId;
    private string _updatedTitle = null!;
    private string _addedRecipeName = null!;

    public WVT127Steps(SharedDriver shared)
    {
        _shared = shared;
    }

    // Given

    [Given("a user is on the edit meal page for meal id {int}")]
    public void GivenAUserIsOnTheEditMealPageForMealId(int mealId)
    {
        _mealId = mealId;
        _shared.Driver.Navigate().GoToUrl($"{SharedDriver.BaseUrl}/Meal/EditMeal?id={mealId}");

        _shared.Wait.Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState")
                .ToString() == "complete");

        _titleInput = _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#mealTitle"));
                return (el.Displayed && el.Enabled) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;

        ((IJavaScriptExecutor)_shared.Driver)
            .ExecuteScript("arguments[0].scrollIntoView(true);", _titleInput);
    }

    // When

    [When("User updates the meal title")]
    public void WhenUserUpdatesTheMealTitle()
    {
        _updatedTitle = "New Meal Title " + DateTime.Now.Ticks;
        _titleInput.Clear();
        _titleInput.SendKeys(_updatedTitle);
    }

    [When("User saves the meal")]
    public void WhenUserSavesTheMeal()
    {
        var saveButton = _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#editMealForm button[type='submit']"));
                return (el.Displayed && el.Enabled) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;

        ((IJavaScriptExecutor)_shared.Driver)
            .ExecuteScript("arguments[0].scrollIntoView(true);", saveButton);

        Thread.Sleep(300);
        saveButton.Click();

        _shared.Wait.Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState")
                .ToString() == "complete");
    }

    [When("User searches for a recipe {string}")]
    public void WhenUserSearchesForARecipe(string searchTerm)
    {
        var searchInput = _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#searchText"));
                return (el.Displayed && el.Enabled) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;

        ((IJavaScriptExecutor)_shared.Driver)
            .ExecuteScript("arguments[0].scrollIntoView(true);", searchInput);

        searchInput.Clear();
        searchInput.SendKeys(searchTerm);

        Thread.Sleep(1100); // wait for throttled search to fire
    }

    [When("User clicks the first search result")]
    public void WhenUserClicksTheFirstSearchResult()
    {
        var firstResult = _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector(".recipeSearchRow"));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;

        ((IJavaScriptExecutor)_shared.Driver)
            .ExecuteScript("arguments[0].scrollIntoView(true);", firstResult);

        Thread.Sleep(300);

        _addedRecipeName = firstResult.FindElement(By.CssSelector(".recipeName")).Text;
        firstResult.Click();
    }

    // Then

    [Then("the updated meal title is shown immediately")]
    public void ThenTheUpdatedMealTitleIsShownImmediately()
    {
        _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#mealTitle"));
                return (el.Displayed && el.GetAttribute("value") == _updatedTitle) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    [Then("the meal is saved with the updated title")]
    public void ThenTheMealIsSavedWithTheUpdatedTitle()
    {
        _shared.Driver.Navigate().GoToUrl($"{SharedDriver.BaseUrl}/Meal/EditMeal?id={_mealId}");

        _shared.Wait.Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState")
                .ToString() == "complete");

        _shared.Wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#mealTitle"));
                return (el.Displayed && el.GetAttribute("value") == _updatedTitle) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    [Then("the recipe is shown in the meal recipe list")]
    public void ThenTheRecipeIsShownInTheMealRecipeList()
    {
        _shared.Wait.Until(driver =>
        {
            var items = driver.FindElements(By.CssSelector("#mealRecipeList .mealRecipeItem"));
            return items.Any(item => item.Text.Contains(_addedRecipeName));
        });
    }

    [Then("the recipe is saved with the meal")]
    public void ThenTheRecipeIsSavedWithTheMeal()
    {
        _shared.Wait.Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState")
                .ToString() == "complete");

        _shared.Wait.Until(driver =>
        {
            var items = driver.FindElements(By.CssSelector(".list-group .buttonGrey h3"));
            return items.Any(item => item.Text.Contains(_addedRecipeName));
        });
    }
}