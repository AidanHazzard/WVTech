using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT101Steps
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly string _baseUrl;

    private string _nutRecipeName = "NutTestDish";

    public WVT101Steps()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    private Tag EnsureTag(MealPlannerDBContext ctx, string name)
    {
        var tag = ctx.Tags.FirstOrDefault(t => t.Name == name);
        if (tag == null)
        {
            tag = new Tag { Name = name };
            ctx.Tags.Add(tag);
            ctx.SaveChanges();
        }
        return tag;
    }

    private void ClearRestrictions(string userName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Users.First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());
        var existing = ctx.UserDietaryRestrictions.Where(udr => udr.UserId == user.Id).ToList();
        ctx.UserDietaryRestrictions.RemoveRange(existing);
        ctx.SaveChanges();
    }

    private void AssignRestriction(string userName, string restrictionName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Users.First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var restriction = ctx.DietaryRestrictions.FirstOrDefault(r => r.Name == restrictionName);
        if (restriction == null)
        {
            restriction = new DietaryRestriction { Name = restrictionName };
            ctx.DietaryRestrictions.Add(restriction);
            ctx.SaveChanges();
        }

        var existing = ctx.UserDietaryRestrictions
            .FirstOrDefault(udr => udr.UserId == user.Id && udr.DietaryRestrictionId == restriction.Id);
        if (existing == null)
        {
            ctx.UserDietaryRestrictions.Add(new UserDietaryRestriction
            {
                UserId = user.Id,
                DietaryRestrictionId = restriction.Id
            });
            ctx.SaveChanges();
        }
    }

    private void NavigateToSearch()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/SearchRecipes");
        _wait.Until(d => d.Url.Contains("SearchRecipes"));
    }

    private void TriggerSearch(string term)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript(
            @"var el = document.getElementById('searchText');
              el.value = arguments[0];
              el.dispatchEvent(new Event('input', { bubbles: true }));",
            term);
    }

    [Given("{string} has a {string} dietary restriction active")]
    public void GivenUserHasDietaryRestrictionActive(string userName, string restrictionName)
    {
        ClearRestrictions(userName);
        AssignRestriction(userName, restrictionName);
    }

    [Given("{string} also has a {string} dietary restriction active")]
    public void GivenUserAlsoHasDietaryRestrictionActive(string userName, string restrictionName)
    {
        AssignRestriction(userName, restrictionName);
    }

    [Given("there is a recipe named {string} that contains a nut-based ingredient")]
    public void GivenRecipeWithNuts(string recipeName)
    {
        _nutRecipeName = recipeName;
        var ctx = BDDSetup.Context;
        ctx.Recipes.Add(new Recipe { Name = recipeName, Directions = "Test", Calories = 300 });
        ctx.SaveChanges();
    }

    [Given("there is a recipe named {string} that contains no nut-based ingredients")]
    public void GivenRecipeWithoutNuts(string recipeName)
    {
        var ctx = BDDSetup.Context;
        var nutTag = EnsureTag(ctx, "Nut Allergy");
        ctx.Recipes.Add(new Recipe { Name = recipeName, Directions = "Test", Calories = 250, Tags = [nutTag] });
        ctx.SaveChanges();
    }

    [Given("there is a recipe named {string} that contains no nuts but contains gluten")]
    public void GivenRecipeNutFreeButGluten(string recipeName)
    {
        var ctx = BDDSetup.Context;
        var nutTag = EnsureTag(ctx, "Nut Allergy");
        ctx.Recipes.Add(new Recipe { Name = recipeName, Directions = "Test", Calories = 300, Tags = [nutTag] });
        ctx.SaveChanges();
    }

    [Given("there is a recipe named {string} that contains no nuts and no gluten")]
    public void GivenRecipeNutFreeAndGlutenFree(string recipeName)
    {
        var ctx = BDDSetup.Context;
        var nutTag = EnsureTag(ctx, "Nut Allergy");
        var glutenTag = EnsureTag(ctx, "Gluten-Free");
        ctx.Recipes.Add(new Recipe { Name = recipeName, Directions = "Test", Calories = 250, Tags = [nutTag, glutenTag] });
        ctx.SaveChanges();
    }

    [Given("every available recipe contains a nut-based ingredient")]
    public void GivenEveryRecipeContainsNuts()
    {
        _nutRecipeName = "NutTestDish";
        var ctx = BDDSetup.Context;
        ctx.Recipes.Add(new Recipe { Name = _nutRecipeName, Directions = "Test", Calories = 400 });
        ctx.SaveChanges();
    }

    [When("{string} searches for {string}")]
    public void WhenUserSearchesFor(string userName, string searchTerm)
    {
        NavigateToSearch();
        TriggerSearch(searchTerm);
        _wait.Until(d =>
            d.FindElements(By.CssSelector(".recipeSearchRow")).Count > 0 ||
            (d.FindElement(By.Id("error")).Displayed &&
             !string.IsNullOrWhiteSpace(d.FindElement(By.Id("error")).Text)));
    }

    [When("{string} searches for recipes")]
    public void WhenUserSearchesForRecipes(string userName)
    {
        NavigateToSearch();
        TriggerSearch(_nutRecipeName);
        _wait.Until(d =>
            d.FindElements(By.CssSelector(".recipeSearchRow")).Count > 0 ||
            (d.FindElement(By.Id("error")).Displayed &&
             !string.IsNullOrWhiteSpace(d.FindElement(By.Id("error")).Text)));
    }

    [Then("{string} appears in the search results")]
    public void ThenRecipeAppearsInResults(string recipeName)
    {
        var names = _driver.FindElements(By.CssSelector(".recipeSearchRow .recipeName"));
        Assert.That(
            names.Any(el => el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{recipeName}' in search results but it was not found");
    }

    [Then("{string} does not appear in the search results")]
    public void ThenRecipeDoesNotAppearInResults(string recipeName)
    {
        var names = _driver.FindElements(By.CssSelector(".recipeSearchRow .recipeName"));
        Assert.That(
            names.All(el => !el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{recipeName}' NOT in search results but it was found");
    }

    [Then("the {string} recipe card displays a {string} tag")]
    public void ThenRecipeCardDisplaysTag(string recipeName, string tagName)
    {
        var row = _wait.Until(d =>
            d.FindElements(By.CssSelector(".recipeSearchRow"))
             .FirstOrDefault(r => r.FindElement(By.CssSelector(".recipeName"))
                                   .Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)));

        Assert.That(row, Is.Not.Null, $"Could not find recipe card for '{recipeName}'");

        var tags = row!.FindElements(By.CssSelector(".restriction-tag"));
        Assert.That(
            tags.Any(t => t.Text.Contains(tagName, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected tag '{tagName}' on '{recipeName}' card but it was not found");
    }

    [Then("a message is displayed explaining no results match the active filters")]
    public void ThenEmptyStateMessageIsDisplayed()
    {
        var error = _wait.Until(d =>
        {
            var el = d.FindElement(By.Id("error"));
            return el.Displayed && !string.IsNullOrWhiteSpace(el.Text) ? el : null;
        });
        Assert.That(error, Is.Not.Null, "Empty state error message was not visible");
        Assert.That(error!.Text, Does.Contain("dietary").IgnoreCase
            .Or.Contain("filter").IgnoreCase.Or.Contain("restriction").IgnoreCase);
    }

    [Then("a link to the dietary restrictions settings page is visible")]
    public void ThenLinkToDietarySettingsIsVisible()
    {
        var link = _wait.Until(d =>
            d.FindElements(By.TagName("a"))
             .FirstOrDefault(a => a.GetAttribute("href")?.Contains("/UserSettings/Dietary") == true));

        Assert.That(link, Is.Not.Null, "Could not find a link to /UserSettings/Dietary");
        Assert.That(link!.Displayed, Is.True);
    }
}
