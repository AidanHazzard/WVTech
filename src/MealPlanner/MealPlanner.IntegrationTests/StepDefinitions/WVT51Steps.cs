using MealPlanner.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT51Steps
{
    private IWebDriver _driver = null!;
    private string _baseUrl = null!;
    private WebDriverWait _wait = null!;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    [Given("'Gary' has completed meals with nutrition data in the past 30 days")]
    public void GivenGaryHasCompletedMealsInThePast30Days()
    {
        using var ctx = BDDSetup.CreateContext();
        var user = ctx.Users.First(u => u.NormalizedEmail == "GARY@FAKEEMAIL.COM");

        var existingCompletions = ctx.MealCompletions
            .Where(mc => mc.Meal.UserId == user.Id && mc.Meal.Title.StartsWith("WVT51 Meal Day"))
            .ToList();
        if (existingCompletions.Count > 0)
        {
            ctx.MealCompletions.RemoveRange(existingCompletions);
            ctx.SaveChanges();
        }

        var existingMeals = ctx.Meals
            .Where(m => m.UserId == user.Id && m.Title.StartsWith("WVT51 Meal Day"))
            .ToList();
        if (existingMeals.Count > 0)
        {
            ctx.Meals.RemoveRange(existingMeals);
            ctx.SaveChanges();
        }

        var recipe = ctx.Recipes.FirstOrDefault(r => r.Name == "WVT51 Test Recipe")
            ?? new Recipe
            {
                Name = "WVT51 Test Recipe",
                Directions = "Test",
                Calories = 500,
                Protein = 30,
                Carbs = 60,
                Fat = 20
            };
        if (recipe.Id == 0)
        {
            ctx.Recipes.Add(recipe);
            ctx.SaveChanges();
        }

        for (int i = 0; i < 7; i++)
        {
            var mealDate = DateTime.Today.AddDays(-i);
            var meal = new Meal
            {
                UserId = user.Id,
                Title = $"WVT51 Meal Day -{i}",
                StartTime = mealDate,
                Recipes = [recipe]
            };
            ctx.Meals.Add(meal);
            ctx.SaveChanges();

            ctx.MealCompletions.Add(new MealCompletion
            {
                MealId = meal.Id,
                CompletionDate = mealDate
            });
        }

        ctx.SaveChanges();
    }

    [Then("the {string} tab is active")]
    public void ThenTheTabIsActive(string tabLabel)
    {
        var tabId = tabLabel.ToLower() == "weekly" ? "tab-weekly" : "tab-monthly";
        var tab = _wait.Until(d => d.FindElement(By.Id(tabId)));
        Assert.That(tab.GetAttribute("class"), Does.Contain("active"),
            $"Expected the '{tabLabel}' tab to have the 'active' class");
    }

    [Then("the {string} tab is visible")]
    public void ThenTheTabIsVisible(string tabLabel)
    {
        var tabId = tabLabel.ToLower() == "weekly" ? "tab-weekly" : "tab-monthly";
        var tab = _wait.Until(d =>
        {
            var el = d.FindElement(By.Id(tabId));
            return el.Displayed ? el : null;
        });
        Assert.That(tab, Is.Not.Null, $"Expected the '{tabLabel}' tab to be visible");
    }

    [When("'Gary' clicks the {string} tab")]
    [Given("'Gary' clicks the {string} tab")]
    public void WhenGaryClicksTheTab(string tabLabel)
    {
        var tabId = tabLabel.ToLower() == "weekly" ? "tab-weekly" : "tab-monthly";
        var tab = _wait.Until(d => d.FindElement(By.Id(tabId)));
        tab.Click();
        _wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");
    }

    [Given("'Gary' is viewing the monthly nutrition report")]
    public void GivenGaryIsViewingTheMonthlyNutritionReport()
    {
        WhenGaryClicksTheTab("Monthly");
    }
}
