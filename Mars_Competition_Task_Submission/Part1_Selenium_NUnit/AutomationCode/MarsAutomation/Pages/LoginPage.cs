using MarsAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace MarsAutomation.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;

    private readonly By _emailInput = By.CssSelector(
        "input[type='email'], input[name='email'], input[name='Email'], input[id='email'], input[id='Email'], input[name*='email'], input[id*='email'], input[placeholder*='Email']");

    private readonly By _passwordInput = By.CssSelector(
        "input[type='password'], input[name='password'], input[name='Password'], input[id='password'], input[id='Password'], input[name*='password'], input[id*='password'], input[placeholder*='Password']");

    private readonly By _loginButton = By.XPath("//button[normalize-space()='Login']");
    private readonly By _errorMessage = By.CssSelector(".alert-danger, .validation-summary-errors, .validation-summary-errors li, .field-validation-error, .ui.red.message, .message.error, .text-danger");
    private readonly By _signInButton = By.XPath("//*[self::a or self::button][normalize-space()='Sign In' or normalize-space()='Login']");
    private readonly By _modalOverlay = By.CssSelector(".ui.page.modals.dimmer.visible.active, .ui.dimmer.visible.active");
    private readonly By _visibleModal = By.CssSelector(".ui.modal.visible.active");

    // Adjust if your app uses different logged-in indicators
    private readonly By _signOutButton = By.XPath("//*[normalize-space()='Sign Out' or normalize-space()='Logout']");
    private readonly By _profileIndicator = By.XPath("//a[contains(@href,'/Account/Profile')] | //a[contains(@href,'/Profile')]");

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public void LoginIfNeeded(string username, string password)
    {
        if (IsLoggedIn())
        {
            DismissModalIfPresent();
            return;
        }

        EnsureLoginFormVisible();

        Login(username, password);
        WaitForLoginToComplete();
    }

    public void Login(string username, string password)
    {
        EnsureLoginFormVisible();

        var email = WaitHelper.WaitForElementVisible(_driver, _emailInput);
        email.Clear();
        email.SendKeys(username);

        var passwordField = WaitHelper.WaitForElementVisible(_driver, _passwordInput);
        passwordField.Clear();
        passwordField.SendKeys(password);

        WaitHelper.WaitForElementClickable(_driver, _loginButton).Click();
    }

    public void EnsureLoginFormVisible()
    {
        if (IsLoginFormVisible())
            return;

        OpenLoginModal();
        WaitHelper.WaitForElementVisible(_driver, _emailInput, 10);
        WaitHelper.WaitForElementVisible(_driver, _passwordInput, 10);
    }

    public void OpenLoginModal()
    {
        if (IsLoginFormVisible())
            return;

        var signInButtons = _driver.FindElements(_signInButton).Where(e => e.Displayed).ToList();
        if (!signInButtons.Any())
        {
            throw new NoSuchElementException("Could not find a visible Sign In/Login button to open the login form.");
        }

        try
        {
            signInButtons.First().Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", signInButtons.First());
        }
    }

    public void LogoutIfNeeded()
    {
        var signOutButtons = _driver.FindElements(_signOutButton).Where(e => e.Displayed).ToList();
        if (!signOutButtons.Any())
            return;

        try
        {
            signOutButtons.First().Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", signOutButtons.First());
        }

        WaitHelper.WaitUntil(_driver, _ => !IsLoggedIn(), 10);
    }

    public bool IsLoggedIn()
    {
        try
        {
            if (_driver.FindElements(_signOutButton).Any(e => e.Displayed))
                return true;

            if (_driver.FindElements(_profileIndicator).Any(e => e.Displayed))
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsLoginFormVisible()
    {
        try
        {
            return _driver.FindElements(_emailInput).Any(e => e.Displayed) &&
                   _driver.FindElements(_passwordInput).Any(e => e.Displayed);
        }
        catch
        {
            return false;
        }
    }

    public void WaitForLoginToComplete()
    {
        WaitHelper.WaitUntil(_driver, driver =>
        {
            bool loggedIn =
                driver.FindElements(_signOutButton).Any(e => e.Displayed) ||
                driver.FindElements(_profileIndicator).Any(e => e.Displayed);

            bool loginFormVisible =
                driver.FindElements(_emailInput).Any(e => e.Displayed) ||
                driver.FindElements(_passwordInput).Any(e => e.Displayed);

            return loggedIn || !loginFormVisible;
        }, 10);

        DismissModalIfPresent();
    }

    public string GetErrorMessage()
    {
        try
        {
            return WaitHelper.WaitForElementVisible(_driver, _errorMessage, 5).Text;
        }
        catch (WebDriverTimeoutException)
        {
            var modal = _driver.FindElements(_visibleModal).FirstOrDefault(e => e.Displayed);
            if (modal != null && !IsLoggedIn())
            {
                var text = modal.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }

            throw;
        }
    }

    public void DismissModalIfPresent()
    {
        try
        {
            var overlays = _driver.FindElements(_modalOverlay).Where(e => e.Displayed).ToList();
            var modals = _driver.FindElements(_visibleModal).Where(e => e.Displayed).ToList();
            if (!overlays.Any() && !modals.Any())
                return;

            try
            {
                new Actions(_driver).SendKeys(Keys.Escape).Perform();
                Thread.Sleep(200);
            }
            catch
            {
                // Keep going and try direct cleanup below.
            }

            overlays = _driver.FindElements(_modalOverlay).Where(e => e.Displayed).ToList();
            modals = _driver.FindElements(_visibleModal).Where(e => e.Displayed).ToList();
            if (!overlays.Any() && !modals.Any())
                return;

            foreach (var overlay in overlays)
            {
                try
                {
                    overlay.Click();
                    Thread.Sleep(150);
                }
                catch
                {
                    // Ignore and continue with JS fallback.
                }
            }

            overlays = _driver.FindElements(_modalOverlay).Where(e => e.Displayed).ToList();
            modals = _driver.FindElements(_visibleModal).Where(e => e.Displayed).ToList();
            if (!overlays.Any() && !modals.Any())
                return;

            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                document.querySelectorAll('.ui.page.modals.dimmer.visible.active, .ui.dimmer.visible.active, .ui.modal.visible.active')
                    .forEach(el => {
                        el.classList.remove('active');
                        el.classList.remove('visible');
                        el.style.display = 'none';
                    });
            ");
        }
        catch
        {
            // Ignore modal cleanup failures and let downstream waits surface real issues.
        }
    }
}

