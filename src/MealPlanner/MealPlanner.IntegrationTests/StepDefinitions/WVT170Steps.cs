using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT170Steps
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;
    private string _baseUrl = null!;

    private int _mealId;
    private string _replacedRecipeName = string.Empty;
    private int _replacedRecipeId;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    // ── DB setup helpers ──────────────────────────────────────────────────────

    [Given("{string} has a meal named {string} containing recipes {string} and {string}")]
    public void GivenUserHasMealContainingTwoRecipes(string userName, string mealTitle, string recipe1Name, string recipe2Name)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var existing = ctx.Set<Meal>().Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.Title == mealTitle).FirstOrDefault();
        if (existing != null) { ctx.Remove(existing); ctx.SaveChanges(); }

        var r1 = new Recipe { Name = recipe1Name, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30 };
        var r2 = new Recipe { Name = recipe2Name, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30 };
        ctx.AddRange(r1, r2);
        ctx.SaveChanges();

        var meal = new Meal { UserId = user.Id, Title = mealTitle, StartTime = DateTime.Today, Recipes = [r1, r2] };
        ctx.Add(meal);
        ctx.SaveChanges();
        _mealId = meal.Id;
    }

    [Given("{string} has a meal named {string} containing only a recipe {string}")]
    public void GivenUserHasMealContainingOnlyOneRecipe(string userName, string mealTitle, string recipeName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var existing = ctx.Set<Meal>().Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.Title == mealTitle).FirstOrDefault();
        if (existing != null) { ctx.Remove(existing); ctx.SaveChanges(); }

        var recipe = new Recipe { Name = recipeName, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30 };
        ctx.Add(recipe);
        ctx.SaveChanges();

        var meal = new Meal { UserId = user.Id, Title = mealTitle, StartTime = DateTime.Today, Recipes = [recipe] };
        ctx.Add(meal);
        ctx.SaveChanges();
        _mealId = meal.Id;
    }

    [Given("{string} has an upvoted recipe named {string} that is not in {string}")]
    public void GivenUserHasUpvotedRecipeNotInMeal(string userName, string recipeName, string mealTitle)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var recipe = new Recipe { Name = recipeName, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30 };
        ctx.Add(recipe);
        ctx.SaveChanges();
        ctx.Add(new UserRecipe { UserId = user.Id, RecipeId = recipe.Id, UserOwner = false, UserVote = UserVoteType.UpVote });
        ctx.SaveChanges();
    }

    [Given("{string} has no other eligible recipes to replace {string}")]
    public void GivenUserHasNoOtherEligibleRecipes(string userName, string recipeName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var meal = ctx.Set<Meal>().Include(m => m.Recipes)
            .First(m => m.UserId == user.Id && m.Id == _mealId);
        var mealRecipeIds = meal.Recipes.Select(r => r.Id).ToHashSet();

        var otherVotes = ctx.Set<UserRecipe>()
            .Where(ur => ur.UserId == user.Id && !mealRecipeIds.Contains(ur.RecipeId))
            .ToList();
        ctx.Set<UserRecipe>().RemoveRange(otherVotes);
        ctx.SaveChanges();

        var otherRecipes = ctx.Set<Recipe>()
            .Where(r => !mealRecipeIds.Contains(r.Id) && r.Id > 0)
            .ToList();
        ctx.Set<Recipe>().RemoveRange(otherRecipes);
        ctx.SaveChanges();
    }

    [Given("{string} has a meal named {string} containing a recipe {string} tagged {string} and a recipe {string} tagged {string}")]
    public void GivenUserHasMealWithTwoTaggedRecipes(string userName, string mealTitle, string recipe1Name, string tag1Name, string recipe2Name, string tag2Name)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var existing = ctx.Set<Meal>().Include(m => m.Recipes)
            .Where(m => m.UserId == user.Id && m.Title == mealTitle).FirstOrDefault();
        if (existing != null) { ctx.Remove(existing); ctx.SaveChanges(); }

        var tag1 = ctx.Set<Tag>().FirstOrDefault(t => t.Name == tag1Name);
        if (tag1 == null) { tag1 = new Tag { Name = tag1Name }; ctx.Add(tag1); ctx.SaveChanges(); }

        var tag2 = ctx.Set<Tag>().FirstOrDefault(t => t.Name == tag2Name);
        if (tag2 == null) { tag2 = new Tag { Name = tag2Name }; ctx.Add(tag2); ctx.SaveChanges(); }

        var r1 = new Recipe { Name = recipe1Name, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30, Tags = [tag1] };
        var r2 = new Recipe { Name = recipe2Name, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30, Tags = [tag2] };
        ctx.AddRange(r1, r2);
        ctx.SaveChanges();

        var meal = new Meal { UserId = user.Id, Title = mealTitle, StartTime = DateTime.Today, Recipes = [r1, r2] };
        ctx.Add(meal);
        ctx.SaveChanges();
        _mealId = meal.Id;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    [When("{string} opens the meal detail page for {string}")]
    public void WhenUserOpensMealDetailPage(string userName, string mealTitle)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());
        var meal = ctx.Set<Meal>().First(m => m.UserId == user.Id && m.Title == mealTitle);
        _mealId = meal.Id;
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/ViewMeal/{meal.Id}");
        _wait.Until(d => d.Url.Contains("/Meal/ViewMeal"));
    }

    [When("{string} returns to the meal detail page for {string}")]
    public void WhenUserReturnsToMealDetailPage(string userName, string mealTitle)
    {
        WhenUserOpensMealDetailPage(userName, mealTitle);
    }

    // ── Regenerate button visibility ──────────────────────────────────────────

    [Then("a regenerate button is visible next to {string}")]
    public void ThenRegenerateButtonIsVisibleNextTo(string recipeName)
    {
        var row = FindRecipeRow(recipeName);
        Assert.That(row, Is.Not.Null, $"Could not find recipe row for '{recipeName}'");
        var btn = row!.FindElements(By.CssSelector("[data-action='regenerate-recipe']")).FirstOrDefault();
        Assert.That(btn, Is.Not.Null, $"No regenerate button found in row for '{recipeName}'");
        Assert.That(btn!.Displayed, Is.True, $"Regenerate button is not displayed for '{recipeName}'");
    }

    [Then("no regenerate button is visible next to {string}")]
    public void ThenNoRegenerateButtonIsVisibleNextTo(string recipeName)
    {
        var row = FindRecipeRow(recipeName);
        Assert.That(row, Is.Not.Null, $"Could not find recipe row for '{recipeName}'");
        var btns = row!.FindElements(By.CssSelector("[data-action='regenerate-recipe']"));
        Assert.That(btns.All(b => !b.Displayed), Is.True,
            $"Expected no visible regenerate button for '{recipeName}' but found one");
    }

    [Then("a regenerate button is visible next to each recipe in that meal")]
    public void ThenRegenerateButtonIsVisibleNextToEachRecipe()
    {
        var rows = _driver.FindElements(By.CssSelector(".mealRecipeItem"));
        Assert.That(rows.Count, Is.GreaterThan(1), "Expected more than one recipe in the meal");
        foreach (var row in rows)
        {
            var btn = row.FindElements(By.CssSelector("[data-action='regenerate-recipe']")).FirstOrDefault();
            Assert.That(btn, Is.Not.Null, "A recipe row is missing a regenerate button");
            Assert.That(btn!.Displayed, Is.True, "A recipe row's regenerate button is not displayed");
        }
    }

    // ── Regenerate action ─────────────────────────────────────────────────────

    [When("{string} clicks regenerate next to {string}")]
    public void WhenUserClicksRegenerateNextTo(string userName, string recipeName)
    {
        var row = FindRecipeRow(recipeName);
        Assert.That(row, Is.Not.Null, $"Could not find recipe row for '{recipeName}'");
        _replacedRecipeName = recipeName;
        _replacedRecipeId = int.Parse(row!.GetAttribute("data-recipe-id") ?? "0");

        var btn = row.FindElement(By.CssSelector("[data-action='regenerate-recipe']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);

        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("{string} clicks the undo option for the regeneration")]
    public void WhenUserClicksUndoForRegeneration(string userName)
    {
        var undoBtn = _wait.Until(d =>
        {
            try { return d.FindElements(By.CssSelector("[data-action='undo-regenerate-recipe']")).FirstOrDefault(e => e.Displayed); }
            catch { return null; }
        });
        Assert.That(undoBtn, Is.Not.Null, "Could not find undo button after regeneration");
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", undoBtn!);
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("no undo option is visible on the meal detail page")]
    public void ThenNoUndoOptionIsVisible()
    {
        var btns = _driver.FindElements(By.CssSelector("[data-action='undo-regenerate-recipe']"));
        Assert.That(btns.All(b => !b.Displayed), Is.True,
            "Expected no undo button to be visible but found one");
    }

    // ── Recipe presence assertions ────────────────────────────────────────────

    [Then("the meal still contains a recipe named {string}")]
    public void ThenMealStillContainsRecipe(string recipeName) => ThenMealContainsRecipe(recipeName);

    [Then("the meal contains a recipe named {string}")]
    public void ThenMealContainsRecipe(string recipeName)
    {
        var rows = _driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
        Assert.That(rows.Any(r => r.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected to find recipe '{recipeName}' in the meal but did not");
    }

    [Then("the meal no longer contains a recipe named {string}")]
    public void ThenMealNoLongerContainsRecipe(string recipeName) => ThenMealDoesNotContainRecipe(recipeName);

    [Then("the meal does not contain a recipe named {string}")]
    public void ThenMealDoesNotContainRecipe(string recipeName)
    {
        var rows = _driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
        Assert.That(rows.All(r => !r.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected recipe '{recipeName}' NOT to be in the meal but it was found");
    }

    [Then("the meal contains a different recipe in place of {string}")]
    public void ThenMealContainsDifferentRecipeInPlaceOf(string replacedName)
    {
        var rows = _driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
        Assert.That(rows.Any(r => !r.Text.Contains(replacedName, StringComparison.OrdinalIgnoreCase)), Is.True,
            "Expected at least one recipe different from the replaced one");
    }

    [Then("{string} sees a message that no alternative recipe is available")]
    public void ThenUserSeesNoAlternativeMessage(string userName)
    {
        var msg = _wait.Until(d =>
        {
            try { return d.FindElements(By.CssSelector("[data-regen-no-alternative]")).FirstOrDefault(e => e.Displayed); }
            catch { return null; }
        });
        Assert.That(msg, Is.Not.Null, "Expected a 'no alternative' message but did not find one");
    }

    // ── Day plan summary ──────────────────────────────────────────────────────

    [Given("the first meal in the summary contains a recipe named {string}")]
    public void GivenFirstMealInSummaryContainsRecipe(string recipeName)
    {
        var items = _driver.FindElements(By.CssSelector("#day-plan-summary .mealRecipeItem h4"));
        Assert.That(items.Any(el => el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected recipe '{recipeName}' in first meal of summary but did not find it");
    }

    [Given("{string} has an upvoted recipe named {string} that is not in the day plan")]
    public void GivenUserHasUpvotedRecipeNotInDayPlan(string userName, string recipeName)
    {
        var ctx = BDDSetup.Context;
        var user = ctx.Set<User>().First(u => u.NormalizedEmail == $"{userName}@fakeemail.com".ToUpper());

        var recipe = new Recipe { Name = recipeName, Directions = "Test", Calories = 300, Protein = 10, Fat = 5, Carbs = 30 };
        ctx.Add(recipe);
        ctx.SaveChanges();
        ctx.Add(new UserRecipe { UserId = user.Id, RecipeId = recipe.Id, UserOwner = false, UserVote = UserVoteType.UpVote });
        ctx.SaveChanges();
    }

    [When("{string} clicks regenerate next to {string} in the summary")]
    public void WhenUserClicksRegenerateNextToInSummary(string userName, string recipeName)
    {
        var items = _driver.FindElements(By.CssSelector("#day-plan-summary .mealRecipeItem"));
        var row = items.FirstOrDefault(r =>
            r.FindElements(By.TagName("h4")).Any(h => h.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)));
        Assert.That(row, Is.Not.Null, $"Could not find recipe row for '{recipeName}' in summary");
        _replacedRecipeName = recipeName;

        var btn = row!.FindElement(By.CssSelector("[data-action='regenerate-recipe']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("the first meal in the summary no longer contains a recipe named {string}")]
    public void ThenFirstMealInSummaryNoLongerContains(string recipeName)
    {
        var items = _driver.FindElements(By.CssSelector("#day-plan-summary .mealRecipeItem h4"));
        Assert.That(items.All(el => !el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected recipe '{recipeName}' to be removed from summary but it was still found");
    }

    [Then("the first meal in the summary contains a different recipe in place of {string}")]
    public void ThenFirstMealInSummaryContainsDifferentRecipe(string replacedName)
    {
        var items = _driver.FindElements(By.CssSelector("#day-plan-summary .mealRecipeItem h4"));
        Assert.That(items.Any(el => !el.Text.Contains(replacedName, StringComparison.OrdinalIgnoreCase)), Is.True,
            "Expected at least one recipe different from the replaced one in the summary");
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private IWebElement? FindRecipeRow(string recipeName) =>
        _driver.FindElements(By.CssSelector(".mealRecipeItem"))
            .FirstOrDefault(r =>
                r.FindElements(By.TagName("h4"))
                 .Any(h => h.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)));
}
