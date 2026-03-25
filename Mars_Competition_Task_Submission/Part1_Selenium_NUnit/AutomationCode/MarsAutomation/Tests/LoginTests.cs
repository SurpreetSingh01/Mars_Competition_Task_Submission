using MarsAutomation.Helpers;
using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using NUnit.Framework;

namespace MarsAutomation.Tests;

public class LoginTests : BaseTest
{
    protected override bool RequiresAuthenticatedSession => false;

    [Test]
    public void SuccessfulLogin_UsingJsonData()
    {
        var profilePage = new ProfilePage(Driver);
        profilePage.Open();
        profilePage.WaitForProfilePageToLoad();

        Assert.That(Driver.Url, Does.Contain("Profile").Or.Contain("Home"));
    }

    [Test]
    public void InvalidLogin_ShowsError()
    {
        var baseUrl = JsonReader.GetString("login", "baseUrl");
        Driver.Navigate().GoToUrl(baseUrl);

        var loginPage = new LoginPage(Driver);
        loginPage.EnsureLoginFormVisible();
        loginPage.Login("invalid@example.com", "wrongpassword");

        var message = loginPage.GetErrorMessage();
        Assert.That(message, Is.Not.Null.Or.Empty);
    }
}

