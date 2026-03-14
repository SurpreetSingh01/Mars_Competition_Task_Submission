using MarsAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Pages;

public class ProfilePage
{
    private readonly IWebDriver _driver;

    // Stable profile markers
    private readonly By _languagesIntroText = By.XPath(
        "//*[contains(normalize-space(),'How many languages do you speak?')]");

    private readonly By _languagesTab = By.XPath(
        "//a[contains(normalize-space(),'Languages')]");

    private readonly By _languagesAddNewButton = By.XPath(
        "//button[normalize-space()='Add New' or @value='Add New']"
        + " | //input[(translate(normalize-space(@value),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='ADD NEW')]");

    // Login form markers
    private readonly By _loginEmailInput = By.CssSelector(
        "input[type='email'], input[name*='email'], input[id*='email']");

    private readonly By _loginPasswordInput = By.CssSelector(
        "input[type='password'], input[name*='password'], input[id*='password']");

    public ProfilePage(IWebDriver driver)
    {
        _driver = driver;
    }

    // Backwards-compatible helper for older tests.
    public void EnsureOnProfilePage()
    {
        Open();
        WaitForProfilePageToLoad();
    }

    public void Open()
    {
        var baseUrl = JsonReader.GetString("login", "baseUrl");
        var profileUrl = baseUrl.TrimEnd('/') + "/Account/Profile";
        _driver.Navigate().GoToUrl(profileUrl);
    }

    public void WaitForProfilePageToLoad()
    {
        // Ensure we land on /Account/Profile (may redirect to login if not authenticated).
        try
        {
            WaitHelper.WaitForUrlContains(_driver, "/Account/Profile", timeoutSeconds: 5);
        }
        catch
        {
            // If we got redirected to login, continue; we'll detect it via form markers.
        }

        // If we see login fields instead of the profile content, perform login once.
        if (IsLoginFormVisible())
        {
            PerformLogin();

            // After login, go back to profile and wait for URL again.
            Open();
            WaitHelper.WaitForUrlContains(_driver, "/Account/Profile", timeoutSeconds: 10);
        }

        // Now wait for at least one stable profile element.
        var loaded = TryWaitForAnyProfileMarker(timeoutSeconds: 10);

        if (!loaded)
        {
            throw new InvalidOperationException(
                "Profile page did not load correctly: could not find Languages text, Languages tab, or Add New button.");
        }
    }

    private bool IsLoginFormVisible()
    {
        try
        {
            var emailFields = _driver.FindElements(_loginEmailInput);
            var passwordFields = _driver.FindElements(_loginPasswordInput);
            return emailFields.Any() && passwordFields.Any();
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    private void PerformLogin()
    {
        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        var loginPage = new LoginPage(_driver);
        loginPage.Login(username, password);
    }

    private bool TryWaitForAnyProfileMarker(int timeoutSeconds)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(500)
        };
        wait.IgnoreExceptionTypes(typeof(WebDriverException));

        try
        {
            return wait.Until(_ =>
                ElementHelper.IsPresent(_driver, _languagesIntroText) ||
                ElementHelper.IsPresent(_driver, _languagesTab) ||
                ElementHelper.IsPresent(_driver, _languagesAddNewButton));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public LanguagesComponent Languages => new LanguagesComponent(_driver);
}

