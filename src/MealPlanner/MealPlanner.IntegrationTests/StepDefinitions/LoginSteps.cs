using System;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class LoginSteps
{
    private readonly SharedDriver _shared;
    private static readonly IConfiguration _config = new ConfigurationBuilder()
        .AddUserSecrets<LoginSteps>()
        .Build();

    public LoginSteps(SharedDriver shared)
    {
        _shared = shared;
    }

    [Given("a user is logged in")]
    public void GivenAUserIsLoggedIn()
    {
        GivenAUserIsLoggedInAs("user");
    }

    [Given("a user is logged in as {string}")]
    public void GivenAUserIsLoggedInAs(string userKey)
    {
        var email = GetCredential(userKey, "Email");
        var password = GetCredential(userKey, "Password");

        _shared.Driver.Navigate().GoToUrl($"{SharedDriver.BaseUrl}/Login");

        _shared.Wait.Until(driver => {
            try { return driver.FindElement(By.CssSelector("input[type='email'], input[name='Email'], input[name='Input.Email']")); }
            catch (NoSuchElementException) { return null; }
        });

        _shared.Driver.FindElement(By.CssSelector("input[type='email'], input[name='Email'], input[name='Input.Email']")).SendKeys(email);
        _shared.Driver.FindElement(By.CssSelector("input[type='password'], input[name='Password'], input[name='Input.Password']")).SendKeys(password);
        _shared.Driver.FindElement(By.CssSelector("button[type='submit'], input[type='submit'], form button")).Click();

        _shared.Wait.Until(driver => !driver.Url.Contains("/Login"));
    }

    private static string GetCredential(string userKey, string field)
    {
        var value = _config[$"TestUsers:{userKey}:{field}"];

        if (string.IsNullOrEmpty(value))
            throw new Exception(
                $"Missing credential '{field}' for user '{userKey}'. " +
                $"Run: dotnet user-secrets set \"TestUsers:{userKey}:{field}\" \"your-value\"");

        return value;
    }
}