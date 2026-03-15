using Reqnroll;

[Binding]
public class ViewRecipeSteps
{
    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("the user is on the Search Recipes page")]
    public void GivenTheUserIsOnTheSearchRecipesPage()
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("the user searchs for {string}")]
    public void GivenTheUserSearchsFor(string s)
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("the user clicks on the first search result")]
    public void WhenTheUserClicksOnTheFirstSearchResult()
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("the user navigates to the {string} recipe page")]
    public void ThenTheUserNavigatesToTheRecipePage(string s)
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("the recipe with id {int} doesnt exist")]
    public void GivenTheRecipeWithIdDoesntExist(int i)
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("the user is redirected to SelectType")]
    public void ThenTheUserIsRedirectedToSelectType()
    {
        // Write code here that turns the phrase above into concrete actions
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("the user navigates to {string}")]
    public void WhenTheUserNavigatesTo(string s)
    {
        // Write code here that turns the phrase above into concrete actions
    }
}