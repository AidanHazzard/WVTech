using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT20Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly WebDriverWait _wait;

    private string _testIngredientName = string.Empty;
    private readonly string _manualItemName = "ManualShoppingItem";
    private int _testMealId;


    public WVT20Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    private void NavigateToShoppingList()
    {
        _driver.Manage().Cookies.DeleteCookieNamed("ShoppingListSynced");
        _driver.Manage().Cookies.DeleteCookieNamed("ShoppingListDateFrom");
        _driver.Manage().Cookies.DeleteCookieNamed("ShoppingListDateTo");
        _driver.Navigate().GoToUrl($"{_baseUrl}/ShoppingList");
        _wait.Until(d => d.Url.Contains("ShoppingList"));
    }

    private string GetAliceId(MealPlannerDBContext ctx) =>
        ctx.Users.First(u => u.Email == "Alice@fakeemail.com").Id;

    private Recipe CreateRecipeWithIngredient(MealPlannerDBContext ctx, string ingredientName)
    {
        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(ib => ib.Name == ingredientName);
        if (ingredientBase == null)
        {
            ingredientBase = new IngredientBase { Name = ingredientName };
            ctx.Set<IngredientBase>().Add(ingredientBase);
            ctx.SaveChanges();
        }

        var measurement = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == "Count");
        if (measurement == null)
        {
            measurement = new Measurement { Name = "Count" };
            ctx.Set<Measurement>().Add(measurement);
            ctx.SaveChanges();
        }

        var recipe = new Recipe
        {
            Name = $"Recipe_{ingredientName}",
            Directions = "Test directions",
            Calories = 100,
            Protein = 5,
            Carbs = 10,
            Fat = 3,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientBase = ingredientBase, Measurement = measurement, Amount = 2 }
            }
        };

        ctx.Recipes.Add(recipe);
        return recipe;
    }

    [Given("'Alice' has an upcoming meal with ingredients")]
    public void GivenAliceHasAnUpcomingMealWithIngredients()
    {
        _testIngredientName = "AliceTestIngredient";

        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);
        var recipe = CreateRecipeWithIngredient(ctx, _testIngredientName);

        var meal = new Meal
        {
            UserId = userId,
            Title = "Alice Test Meal",
            StartTime = DateTime.Today.AddHours(12)
        };
        meal.Recipes.Add(recipe);
        ctx.Meals.Add(meal);
        ctx.SaveChanges();
        _testMealId = meal.Id;
    }

    [When("'Alice' views her shopping list")]
    public void WhenAliceViewsHerShoppingList()
    {
        NavigateToShoppingList();
    }

    [Then("the shopping list contains the ingredients from her upcoming meal")]
    public void ThenTheShoppingListContainsIngredientsFromMeal()
    {
        _wait.Until(d => d.PageSource.Contains(_testIngredientName));
        Assert.That(_driver.PageSource, Does.Contain(_testIngredientName));
    }

    [Given("'Alice' is on the create meal page")]
    public void GivenAliceIsOnTheCreateMealPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
    }

    [When("'Alice' creates a meal with a recipe that has ingredients")]
    public void WhenAliceCreatesAMealWithARecipeThatHasIngredients()
    {
        _testIngredientName = "AliceNewMealIngredient";

        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);
        var recipe = CreateRecipeWithIngredient(ctx, _testIngredientName);

        var meal = new Meal
        {
            UserId = userId,
            Title = "Alice Created Meal",
            StartTime = DateTime.Today.AddHours(12)
        };
        meal.Recipes.Add(recipe);
        ctx.Meals.Add(meal);
        ctx.SaveChanges();
        _testMealId = meal.Id;
    }

    [Then("the ingredients from that recipe appear on her shopping list")]
    public void ThenIngredientsFromRecipeAppearOnShoppingList()
    {
        NavigateToShoppingList();
        _wait.Until(d => d.PageSource.Contains(_testIngredientName));
        Assert.That(_driver.PageSource, Does.Contain(_testIngredientName));
    }

    [When("'Alice' deletes that meal")]
    public void WhenAliceDeletesThatMeal()
    {
        using var ctx = BDDSetup.CreateContext();
        var meal = ctx.Meals.Find(_testMealId);
        if (meal != null)
        {
            ctx.Meals.Remove(meal);
            ctx.SaveChanges();
        }

        _driver.Manage().Cookies.DeleteCookieNamed("ShoppingListSynced");
    }

    [Then("the ingredients from that meal are no longer on her shopping list")]
    public void ThenIngredientsFromMealAreNoLongerOnShoppingList()
    {
        NavigateToShoppingList();
        _wait.Until(d => d.Url.Contains("ShoppingList"));
        Assert.That(_driver.PageSource, Does.Not.Contain(_testIngredientName));
    }

    [Given("'Alice' has two upcoming meals that share an ingredient")]
    public void GivenAliceHasTwoUpcomingMealsThatShareAnIngredient()
    {
        _testIngredientName = "SharedMealIngredient";

        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);

        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(ib => ib.Name == _testIngredientName);
        if (ingredientBase == null)
        {
            ingredientBase = new IngredientBase { Name = _testIngredientName };
            ctx.Set<IngredientBase>().Add(ingredientBase);
            ctx.SaveChanges();
        }

        var measurement = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == "Count");
        if (measurement == null)
        {
            measurement = new Measurement { Name = "Count" };
            ctx.Set<Measurement>().Add(measurement);
            ctx.SaveChanges();
        }

        var recipe1 = new Recipe
        {
            Name = "SharedIngredientRecipe1",
            Directions = "Test",
            Calories = 100, Protein = 5, Carbs = 10, Fat = 3,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientBase = ingredientBase, Measurement = measurement, Amount = 1 }
            }
        };
        var recipe2 = new Recipe
        {
            Name = "SharedIngredientRecipe2",
            Directions = "Test",
            Calories = 100, Protein = 5, Carbs = 10, Fat = 3,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientBase = ingredientBase, Measurement = measurement, Amount = 2 }
            }
        };

        var meal1 = new Meal { UserId = userId, Title = "Shared Meal 1", StartTime = DateTime.Today.AddHours(10) };
        var meal2 = new Meal { UserId = userId, Title = "Shared Meal 2", StartTime = DateTime.Today.AddHours(14) };
        meal1.Recipes.Add(recipe1);
        meal2.Recipes.Add(recipe2);

        ctx.Recipes.AddRange(recipe1, recipe2);
        ctx.Meals.AddRange(meal1, meal2);
        ctx.SaveChanges();
    }

    [Then("that shared ingredient appears only once on the shopping list")]
    public void ThenSharedIngredientAppearsOnlyOnce()
    {
        _wait.Until(d => d.Url.Contains("ShoppingList"));
        var occurrences = _driver.FindElements(By.XPath($"//*[contains(text(), '{_testIngredientName}')]")).Count;
        Assert.That(occurrences, Is.EqualTo(1));
    }

    [Given("'Alice' has manually added an item to her shopping list")]
    public void GivenAliceHasManuallyAddedAnItem()
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);

        ctx.Set<ShoppingListItem>().Add(new ShoppingListItem
        {
            UserId = userId,
            Name = _manualItemName,
            Amount = 1,
            Measurement = "Count",
            IsAutoAdded = false
        });
        ctx.SaveChanges();
    }

    [Then("both the auto-populated ingredients and the manually added item are present")]
    public void ThenBothAutoAndManualItemsArePresent()
    {
        _wait.Until(d => d.Url.Contains("ShoppingList"));
        Assert.That(_driver.PageSource, Does.Contain(_testIngredientName));
        Assert.That(_driver.PageSource, Does.Contain(_manualItemName));
    }

    [Then("the manually added item is still on her shopping list")]
    public void ThenManualItemIsStillOnShoppingList()
    {
        NavigateToShoppingList();
        _wait.Until(d => d.Url.Contains("ShoppingList"));
        Assert.That(_driver.PageSource, Does.Contain(_manualItemName));
    }

    [Then("the shopping list items are saved to the database")]
    public void ThenShoppingListItemsAreSavedToDatabase()
    {
        NavigateToShoppingList();
        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);
        var items = ctx.Set<ShoppingListItem>().Where(i => i.UserId == userId).ToList();
        Assert.That(items.Any(i => i.Name.ToLower() == _testIngredientName.ToLower()), Is.True);
    }

    [Given("'Alice' has an upcoming meal with an ingredient named {string}")]
    public void GivenAliceHasAnUpcomingMealWithAnIngredientNamed(string ingredientName)
    {
        _testIngredientName = ingredientName;

        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);

        var staleItems = ctx.Set<ShoppingListItem>()
            .Where(i => i.UserId == userId && i.Name.ToLower() == ingredientName.ToLower()).ToList();
        ctx.Set<ShoppingListItem>().RemoveRange(staleItems);
        var staleMeals = ctx.Meals
            .Where(m => m.UserId == userId && m.Title == $"{ingredientName} Meal").ToList();
        ctx.Meals.RemoveRange(staleMeals);
        ctx.SaveChanges();

        var ingredientBase = ctx.Set<IngredientBase>().FirstOrDefault(ib => ib.Name == ingredientName);
        if (ingredientBase == null)
        {
            ingredientBase = new IngredientBase { Name = ingredientName };
            ctx.Set<IngredientBase>().Add(ingredientBase);
            ctx.SaveChanges();
        }

        var measurement = ctx.Set<Measurement>().FirstOrDefault(m => m.Name == "Count");
        if (measurement == null)
        {
            measurement = new Measurement { Name = "Count" };
            ctx.Set<Measurement>().Add(measurement);
            ctx.SaveChanges();
        }

        var recipe = new Recipe
        {
            Name = $"{ingredientName}Recipe",
            Directions = "Test",
            Calories = 100, Protein = 5, Carbs = 10, Fat = 3,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientBase = ingredientBase, Measurement = measurement, Amount = 10 }
            }
        };
        ctx.Recipes.Add(recipe);

        var meal = new Meal
        {
            UserId = userId,
            Title = $"{ingredientName} Meal",
            StartTime = DateTime.Today.AddHours(12)
        };
        meal.Recipes.Add(recipe);
        ctx.Meals.Add(meal);
        ctx.SaveChanges();
        _testMealId = meal.Id;
    }

    [Given("'Alice' has manually added {string} to her shopping list")]
    public void GivenAliceHasManuallyAddedNamedItemToShoppingList(string itemName)
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);

        ctx.Set<ShoppingListItem>().Add(new ShoppingListItem
        {
            UserId = userId,
            Name = itemName,
            Amount = 1,
            Measurement = "Count",
            IsAutoAdded = false
        });
        ctx.SaveChanges();
    }

    [Then("{string} appears only once on the shopping list")]
    public void ThenIngredientAppearsOnlyOnce(string ingredientName)
    {
        _wait.Until(d => d.Url.Contains("ShoppingList"));
        var occurrences = _driver.FindElements(
            By.XPath($"//*[contains(@class,'item-display') and contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{ingredientName.ToLower()}')]")
        ).Count;
        Assert.That(occurrences, Is.EqualTo(1));
    }

    [When("'Alice' updates the quantity of '(.*)' to (.*)")]
    public void WhenAliceUpdatesTheQuantityOf(string itemName, float newAmount)
    {
        _wait.Until(d => d.FindElements(By.CssSelector("input[name='newAmount']")).Count > 0);

        var rows = _driver.FindElements(By.CssSelector(".back2-textbox"));
        foreach (var row in rows)
        {
            if (row.Text.Contains(itemName))
            {
                var input = row.FindElement(By.CssSelector("input[name='newAmount']"));
                input.Clear();
                input.SendKeys(newAmount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                row.FindElement(By.CssSelector("button[type='submit']")).Click();
                break;
            }
        }
    }

    [Then("the shopping list shows quantity (.*) for '(.*)'")]
    public void ThenTheShoppingListShowsQuantityFor(float expectedAmount, string itemName)
    {
        NavigateToShoppingList();
        _wait.Until(d => d.Url.Contains("ShoppingList"));

        var input = _wait.Until(d =>
        {
            var rows = d.FindElements(By.CssSelector(".back2-textbox"));
            foreach (var row in rows)
            {
                if (row.Text.Contains(itemName))
                    return row.FindElement(By.CssSelector("input[name='newAmount']"));
            }
            return null;
        });

        Assert.That(input, Is.Not.Null);
        var displayedAmount = float.Parse(
            input!.GetAttribute("value") ?? "0",
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.That(displayedAmount, Is.EqualTo(expectedAmount));
    }

    [Then("the associated shopping list items are removed from the database")]
    public void ThenAssociatedShoppingListItemsAreRemovedFromDatabase()
    {
        NavigateToShoppingList();
        using var ctx = BDDSetup.CreateContext();
        var userId = GetAliceId(ctx);
        var items = ctx.Set<ShoppingListItem>()
            .Where(i => i.UserId == userId && i.IsAutoAdded)
            .ToList();
        Assert.That(items.Any(i => i.Name.ToLower() == _testIngredientName.ToLower()), Is.False);
    }
}
