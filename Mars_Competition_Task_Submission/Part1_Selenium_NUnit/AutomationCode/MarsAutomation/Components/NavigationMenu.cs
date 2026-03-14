using OpenQA.Selenium;
using MarsAutomation.Helpers;
using MarsAutomation.Pages;

namespace MarsAutomation.Components;

public class NavigationMenu
{
    private readonly IWebDriver _driver;

    public NavigationMenu(IWebDriver driver)
    {
        _driver = driver;
    }

    public void GoToProfile()
    {
        // Prefer direct navigation to the profile URL to avoid brittle menu selectors.
        var baseUrl = JsonReader.GetString("login", "baseUrl");
        var profileUrl = baseUrl.TrimEnd('/') + "/Account/Profile";
        _driver.Navigate().GoToUrl(profileUrl);
    }

    public void GoToShareSkill()
    {
        EnsureSessionReady();

        var baseUrl = JsonReader.GetString("login", "baseUrl");
        _driver.Navigate().GoToUrl(baseUrl.TrimEnd('/') + "/Home/ServiceListing");

        EnsureSessionReady();
    }

    public void GoToSearch()
    {
        EnsureSessionReady();

        var baseUrl = JsonReader.GetString("login", "baseUrl");
        _driver.Navigate().GoToUrl(baseUrl);

        EnsureSessionReady();
    }

    private void EnsureSessionReady()
    {
        var loginPage = new LoginPage(_driver);
        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        loginPage.LoginIfNeeded(username, password);
        loginPage.DismissModalIfPresent();
    }
}

