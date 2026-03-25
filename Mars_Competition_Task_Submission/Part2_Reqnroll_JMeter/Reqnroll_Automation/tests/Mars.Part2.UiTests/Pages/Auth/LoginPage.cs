using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Pages.Auth;

public sealed class LoginPage : BasePage
{
    private static readonly By[] EmailInputLocators =
    {
        By.CssSelector("input[name='email']"),
        By.Name("email"),
        By.Name("Email"),
        By.CssSelector("input[type='email']"),
        By.CssSelector("input[placeholder='Email address']"),
        By.CssSelector("input[placeholder='user@email.com']"),
        By.CssSelector("input[placeholder*='email' i]")
    };

    private static readonly By[] PasswordInputLocators =
    {
        By.CssSelector("input[name='password']"),
        By.Name("password"),
        By.Name("Password"),
        By.CssSelector("input[placeholder='Password']"),
        By.CssSelector("input[type='password']")
    };

    private static readonly By[] SignInButtonLocators =
    {
        By.XPath("//button[normalize-space(.)='Login']"),
        By.CssSelector("button[type='submit']"),
        By.XPath("//div[contains(@class,'fluid') and contains(@class,'teal') and normalize-space(.)='Login']"),
        By.XPath("//button[contains(.,'Sign In') or contains(.,'Login') or contains(.,'Log in')]"),
        By.XPath("//input[@type='submit']"),
        By.XPath("//*[self::button or self::div][contains(@class,'teal') and normalize-space(.)='Login']")
    };

    public LoginPage(UiTestContext context) : base(context)
    {
    }

    public bool IsVisible()
    {
        return HasVisibleElement(EmailInputLocators) &&
               HasVisibleElement(PasswordInputLocators) &&
               HasVisibleElement(SignInButtonLocators);
    }

    public bool AppearsWithin(int timeoutSeconds)
    {
        return Wait.TryWaitForCondition(_ => IsVisible(), timeoutSeconds);
    }

    public void Login(Credentials credentials)
    {
        if (!IsVisible())
        {
            throw new InvalidOperationException(
                $"Login was requested, but no login form is visible. Verify this in DOM first. {Context.DebugState.BuildPageStateSummary()}");
        }

        if (string.IsNullOrWhiteSpace(credentials.Email) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            throw new InvalidOperationException("Credentials are empty. Update TestData/credentials.json or set MARS_EMAIL and MARS_PASSWORD.");
        }

        Element.EnterText("email input", credentials.Email, EmailInputLocators);
        Element.EnterText("password input", credentials.Password, PasswordInputLocators);
        Element.Click("sign in button", SignInButtonLocators);

        if (!Wait.TryWaitForCondition(_ => !IsVisible(), 10))
        {
            throw new InvalidOperationException(
                $"Login form remained visible after submitting credentials. Verify the login response in DOM first. {Context.DebugState.BuildPageStateSummary()}");
        }

        WaitUntilReady();
    }

    private bool HasVisibleElement(params By[] locators)
    {
        return locators.Any(locator => Wait.TryWaitForElementVisible(locator, out _, 1));
    }
}
