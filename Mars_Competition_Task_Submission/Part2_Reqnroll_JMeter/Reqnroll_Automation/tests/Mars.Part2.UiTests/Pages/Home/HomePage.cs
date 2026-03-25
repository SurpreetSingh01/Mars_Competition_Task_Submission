using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Auth;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Mars.Part2.UiTests.Pages.Home;

public sealed class HomePage : BasePage
{
    private static readonly By[] SignInTriggerLocators =
    {
        By.XPath("//a[contains(normalize-space(.),'Sign In') or contains(normalize-space(.),'Login') or contains(normalize-space(.),'Log in')]"),
        By.XPath("//button[contains(normalize-space(.),'Sign In') or contains(normalize-space(.),'Login') or contains(normalize-space(.),'Log in')]"),
        By.CssSelector("a[href*='login' i]"),
        By.CssSelector("button[class*='login' i]")
    };

    public HomePage(UiTestContext context) : base(context)
    {
    }

    public void Open()
    {
        Driver.Navigate().GoToUrl(Context.Settings.Application.BaseUrl);
        WaitUntilReady();
    }

    public void SignIn(Credentials credentials)
    {
        Open();

        var loginPage = new LoginPage(Context);
        if (!loginPage.IsVisible())
        {
            TryOpenLoginEntryPoint();
        }

        if (!loginPage.AppearsWithin(10))
        {
            throw new InvalidOperationException(
                $"Login form did not appear from the home page. Verify the home-page sign-in trigger in DOM first. {Context.DebugState.BuildPageStateSummary()}");
        }

        loginPage.Login(credentials);
    }

    private void TryOpenLoginEntryPoint()
    {
        foreach (var locator in SignInTriggerLocators)
        {
            try
            {
                Element.Click("home sign in trigger", locator);
                return;
            }
            catch (InvalidOperationException)
            {
            }
            catch (WebDriverTimeoutException)
            {
            }
        }
    }
}
