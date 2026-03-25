using MarsAutomation.Pages;
using OpenQA.Selenium;

namespace MarsAutomation.Helpers;

public static class SessionHelper
{
    public static bool IsLoggedIn(IWebDriver driver)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        try
        {
            // Positive indicators of an authenticated session.
            if (driver.FindElements(By.LinkText("Sign Out")).Any() ||
                driver.FindElements(By.LinkText("Logout")).Any())
            {
                return true;
            }

            var url = driver.Url ?? string.Empty;
            if (url.Contains("/Account/Profile", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch (WebDriverException)
        {
            // If we can't reliably inspect, assume not logged in.
        }

        return false;
    }

    public static void EnsureLoggedIn(IWebDriver driver)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        if (IsLoggedIn(driver))
        {
            return;
        }

        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        var loginPage = new LoginPage(driver);
        loginPage.Login(username, password);
    }
}

