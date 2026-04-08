using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class WVT43Steps
{
    IWebDriver _driver;
    string _noRecipeFoundMessage = "No recipes found, sorry!";
    ReadOnlyCollection<IWebElement> _oldResults;

    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");


        _driver = new ChromeDriver(options);

        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
    }

    // Runs after each scenerio
    [AfterScenario]
    public void TearDown()
    {
        _driver.Quit();
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("User is on recipe page")]
    public void GivenUserIsOnRecipePage()
    {
        _driver.Navigate().GoToUrl("http://localhost:5124/FoodEntries/Recipes");
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("User types {string} in the search")]
    public void WhenUserTypesInTheSearch(string searchString)
    {
        _driver.FindElement(By.Id("searchText")).SendKeys(searchString);
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
        IWebElement error = _driver.FindElement(By.Id("error"));
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => error.Displayed);
        string message = error.Text;
        Assert.That(message, Is.EqualTo(_noRecipeFoundMessage));
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