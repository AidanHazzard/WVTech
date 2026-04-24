using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT43Steps
{
    IWebDriver _driver;
    string _baseUrl;
    string _noRecipeFoundMessage = "No recipes found, sorry!";
    ReadOnlyCollection<IWebElement> _oldResults;
    
    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("User is on recipe page")]
    public void GivenUserIsOnRecipePage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Recipes");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        // If redirected away (e.g. pending logout navigation clobbered this GoToUrl), retry once
        if (!_driver.Url.Contains("/FoodEntries/Recipes", StringComparison.OrdinalIgnoreCase))
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Recipes");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        }
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("User types {string} in the search")]
    public void WhenUserTypesInTheSearch(string searchString)
    {
        var el = new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(d =>
        {
            try { var e = d.FindElement(By.Id("searchText")); return e.Displayed ? e : null; }
            catch (NoSuchElementException) { return null; }
        })!;
        el.Clear();
        el.SendKeys(searchString);
        Thread.Sleep(1100); // wait for search debounce to fire
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("The search list populates with recipes that have {string} in their name")]
    public void ThenTheSearchListPopulatesWithRecipesThatHaveInTheirName(string searchString)
    {
        var searchResults = _driver.FindElements(By.ClassName("recipeSearchRow"));
        Assert.That(searchResults, Is.Not.Empty);

        foreach (IWebElement result in searchResults)
        {
            Assert.That(result.Text.ToLower(), Does.Contain(searchString.ToLower()));
        }
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("The search results are the same")]
    public void ThenTheSearchResultsAreTheSame()
    {
        var searchResults = _driver.FindElements(By.ClassName("recipeSearchRow"));
        Assert.That(searchResults, Has.Count.EqualTo(_oldResults.Count()));
        for(int i = 0; i < searchResults.Count(); i++)
        {
            string newResult = searchResults[i].Text.ToLower();
            string oldResult = _oldResults[i].Text.ToLower();
            Assert.That(newResult, Is.EqualTo(oldResult));
        }
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("User had searched for {string}")]
    public void GivenUserHadSearchedFor(string searchString)
    {
        _driver.FindElement(By.Id("searchText")).SendKeys(searchString);
        _oldResults = _driver.FindElements(By.ClassName("recipeSearchRow"));
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("User is told there are no recipes found")]
    public void ThenUserIsToldThereAreNoRecipesFound()
    {
        var error = new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(d =>
        {
            try { var el = d.FindElement(By.Id("error")); return el.Displayed ? el : null; }
            catch (NoSuchElementException) { return null; }
            catch (StaleElementReferenceException) { return null; }
        })!;
        Assert.That(error.Text, Is.EqualTo(_noRecipeFoundMessage));
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("The user sees a search bar")]
    public void ThenTheUserSeesASearchBar()
    {
        try
        {
           IWebElement search = _driver.FindElement(By.Id("searchText"));
           Assert.Pass();
        }
        catch (NoSuchElementException)
        {
            Assert.Fail();
        }
    }
}