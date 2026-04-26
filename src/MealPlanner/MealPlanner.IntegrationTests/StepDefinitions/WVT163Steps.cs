using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT163Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly WebDriverWait _wait;
    private int _expectedMealCount;

    public WVT163Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    private string GetDaveId(MealPlannerDBContext ctx) =>
        ctx.Users.First(u => u.Email == "Dave@fakeemail.com").Id;

    [Given("'Dave' has {int} meal scheduled for today")]
    [Given("'Dave' has {int} meals scheduled for today")]
    public void GivenDaveHasMealsScheduledForToday(int count)
    {
        _expectedMealCount = count;

        using var ctx = BDDSetup.CreateContext();
        var userId = GetDaveId(ctx);

        var existing = ctx.Meals.Where(m => m.UserId == userId).ToList();
        ctx.Meals.RemoveRange(existing);
        ctx.SaveChanges();

        for (int i = 0; i < count; i++)
        {
            ctx.Meals.Add(new Meal
            {
                UserId = userId,
                Title = $"WVT163TestMeal{i + 1}",
                StartTime = DateTime.Today.AddHours(i + 1)
            });
        }
        ctx.SaveChanges();
    }

    [When("'Dave' navigates to the home page")]
    public void WhenDaveNavigatesToHomePage()
    {
        _driver.Navigate().GoToUrl(_baseUrl);
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("there is {int} meal card on the home page")]
    [Then("there are {int} meal cards on the home page")]
    public void ThenThereAreNMealCardsOnHomePage(int count)
    {
        _wait.Until(d => d.FindElements(By.CssSelector(".mealCardWrapper")).Count == count);
        var cards = _driver.FindElements(By.CssSelector(".mealCardWrapper"));
        Assert.That(cards.Count, Is.EqualTo(count));
        Assert.That(cards.All(c => c.Displayed), Is.True);
    }

    [Then("all meal card titles are readable on the home page")]
    public void ThenAllMealCardTitlesAreReadable()
    {
        _wait.Until(d => d.FindElements(By.CssSelector(".mealCardWrapper")).Count >= _expectedMealCount);
        var cards = _driver.FindElements(By.CssSelector(".mealCardWrapper"));
        Assert.That(cards.Count, Is.EqualTo(_expectedMealCount));
        foreach (var card in cards)
        {
            var titleEl = card.FindElement(By.CssSelector(".buttonText"));
            Assert.That(titleEl.Displayed, Is.True);
            Assert.That(titleEl.Text.Trim(), Is.Not.Empty);
        }
    }
}
