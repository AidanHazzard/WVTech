using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class NutritionBarSteps
{
    IWebDriver _driver;
    string _baseUrl;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} is on the create recipe page")]
    public void GivenUserIsOnCreateRecipePage(string username)
    {
        EnsureOnCreateRecipePage(username);
    }

    private void EnsureOnCreateRecipePage(string username)
    {
        const int maxAttempts = 3;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/AddNewRecipe");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");

            // Pending login redirect can cause Navigate to land on "/" — retry.
            if (!_driver.Url.Contains("/FoodEntries/AddNewRecipe", StringComparison.OrdinalIgnoreCase))
            {
                if (_driver.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase))
                {
                    _driver.FindElement(By.Id("Email")).SendKeys($"{username}@fakeemail.com");
                    _driver.FindElement(By.Id("Password")).SendKeys("1234!Abcd");
                    _driver.FindElement(By.ClassName("btn")).Click();
                    new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(d =>
                        !d.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
                }
                continue;
            }

            try
            {
                new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(driver =>
                {
                    try { return driver.FindElement(By.Id("Directions")).Displayed; }
                    catch (NoSuchElementException) { return false; }
                    catch (StaleElementReferenceException) { return false; }
                });
                return;
            }
            catch (WebDriverTimeoutException)
            {
                if (attempt == maxAttempts - 1) throw;
            }
        }
    }

    [Given("{string} fills in the recipe name as {string}")]
    public void GivenUserFillsInRecipeName(string username, string name)
    {
        _driver.FindElement(By.Id("Name")).SendKeys(name);
    }

    [Given("{string} fills in the recipe directions as {string}")]
    public void GivenUserFillsInRecipeDirections(string username, string directions)
    {
        _driver.FindElement(By.Id("Directions")).SendKeys(directions);
    }

    [Given("{string} fills in the recipe calories as {string}")]
    public void GivenUserFillsInRecipeCalories(string username, string calories)
    {
        _driver.FindElement(By.Id("Calories")).SendKeys(calories);
    }

    [Given("{string} fills in the recipe protein as {string}")]
    public void GivenUserFillsInRecipeProtein(string username, string protein)
    {
        _driver.FindElement(By.Id("Protein")).SendKeys(protein);
    }

    [Given("{string} fills in the recipe fat as {string}")]
    public void GivenUserFillsInRecipeFat(string username, string fat)
    {
        _driver.FindElement(By.Id("Fat")).SendKeys(fat);
    }

    [Given("{string} fills in the recipe carbs as {string}")]
    public void GivenUserFillsInRecipeCarbs(string username, string carbs)
    {
        _driver.FindElement(By.Id("Carbs")).SendKeys(carbs);
    }

    [Given("{string} submits the recipe form")]
    public void GivenUserSubmitsRecipeForm(string username)
    {
        var btn = _driver.FindElement(By.CssSelector("button.buttonBlue[type='submit']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'center',behavior:'instant'});", btn);
        btn.Click();
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Given("{string} is on the create meal page for nutrition")]
    public void GivenUserIsOnCreateMealPage(string username)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Given("{string} fills in the meal title as {string}")]
    public void GivenUserFillsInMealTitle(string username, string title)
    {
        _driver.FindElement(By.Id("Title")).SendKeys(title);
    }

    [Given("{string} sets the meal date to today")]
    public void GivenUserSetsMealDateToToday(string username)
    {
        var monthDropdown = new SelectElement(
            _driver.FindElement(By.Id("SelectedMonth"))
        );

        var dayDropdown = new SelectElement(
            _driver.FindElement(By.Id("SelectedDay"))
        );

        var today = DateTime.Now;

        monthDropdown.SelectByValue(today.Month.ToString());
        dayDropdown.SelectByValue(today.Day.ToString());
    }

    [Given("{string} searches for recipe {string}")]
    public void GivenUserSearchesForRecipe(string username, string recipeName)
    {
        var searchInput = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.Id("searchText"));
                return (el.Displayed && el.Enabled) ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        })!;

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "arguments[0].scrollIntoView(true);", searchInput);
        searchInput.Click();
        searchInput.Clear();
        searchInput.SendKeys(recipeName);
        Thread.Sleep(1100); // wait for search debounce
    }

    [Given("{string} clicks the first recipe result")]
    public void GivenUserClicksFirstRecipeResult(string username)
    {
        var firstResult = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector(".recipeSearchRow"));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        })!;

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "arguments[0].scrollIntoView(true);", firstResult);
        firstResult.Click();
    }

    [Given("{string} submits the meal form")]
    public void GivenUserSubmitsMealForm(string username)
    {
        _driver.FindElement(By.CssSelector("button[form='createMealForm']")).Click();
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Given("'(.*)' marks the meal as completed")]
    public void GivenUserMarksTheMealAsCompleted(string user)
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/PlannerHome?date={today}");

        var checkbox = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector("input[type='checkbox'][name='isCompleted']")));

        checkbox.Click();

        // Wait for page to reload after form submit
        new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector("input[type='checkbox'][name='isCompleted']")));
    }

    [Then("Meal Bars callories are at {int}\\/{int}")]
    public void ThenMealBarsCaloriesAreAt(int current, int goal)
    {
        var fraction = _driver.FindElement(
            By.CssSelector(".nutrition-bar-row:nth-child(1) .nutrition-bar-fraction"));
        Assert.That(fraction.Text.Trim(), Is.EqualTo($"{current} / {goal}"));
    }

    [Then("Meal Bars protien are at {int}\\/{int}")]
    public void ThenMealBarsProteinAreAt(int current, int goal)
    {
        var fraction = _driver.FindElement(
            By.CssSelector(".nutrition-bar-row:nth-child(2) .nutrition-bar-fraction"));
        Assert.That(fraction.Text.Trim(), Is.EqualTo($"{current} / {goal}"));
    }

    [Then("Meal Bars fats are at {int}\\/{int}")]
    public void ThenMealBarsFatsAreAt(int current, int goal)
    {
        var fraction = _driver.FindElement(
            By.CssSelector(".nutrition-bar-row:nth-child(3) .nutrition-bar-fraction"));
        Assert.That(fraction.Text.Trim(), Is.EqualTo($"{current} / {goal}"));
    }

    [Then("Meal Bars carbs are at {int}\\/{int}")]
    public void ThenMealBarsCarbsAreAt(int current, int goal)
    {
        var fraction = _driver.FindElement(
            By.CssSelector(".nutrition-bar-row:nth-child(4) .nutrition-bar-fraction"));
        Assert.That(fraction.Text.Trim(), Is.EqualTo($"{current} / {goal}"));
    }
    [Given("{string} is on the page {string}")]
    public void GivenUserIsOnPage(string username, string pageName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/{pageName}");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Given("{string} fills in the nutrition targets")]
    public void GivenUserFillsInNutritionTargets(string username)
    {
        _driver.FindElement(By.Id("CalorieTarget")).SendKeys("40");
        _driver.FindElement(By.Id("ProteinTarget")).SendKeys("50");
        _driver.FindElement(By.Id("CarbTarget")).SendKeys("70");
        _driver.FindElement(By.Id("FatTarget")).SendKeys("60");
        _driver.FindElement(By.CssSelector("button.buttonBlue[type='submit']")).Click();
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }
}