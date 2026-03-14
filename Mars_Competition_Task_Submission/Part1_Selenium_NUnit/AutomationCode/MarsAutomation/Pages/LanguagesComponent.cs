using MarsAutomation.Components;
using MarsAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Pages;

/// <summary>
/// Represents the Languages section on the Profile page.
/// </summary>
public class LanguagesComponent
{
    private readonly IWebDriver _driver;
    private readonly CommonActions _actions;

    // Tolerant locator for the Languages "Add New" control
    // (button / input / link / div with text 'Add New').
    private readonly By _addNewButton = By.XPath(
        "//*[self::button or self::input or self::a or self::div]"
        + "[translate(normalize-space(.),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='ADD NEW']");

    private readonly By _languageInput = By.XPath("//input[@name='name']");
    private readonly By _levelDropdown = By.XPath("//select[@name='level']");
    // Use safer, separate locators for Add and Update buttons
    private readonly By _addButton = By.XPath("//input[contains(@class,'button') and @value='Add']");
    private readonly By _updateButton = By.XPath("//input[contains(@class,'button') and @value='Update']");
    private readonly By _languageRowsBy = By.XPath("//div[@data-tab='first']//tbody/tr");

    // Login modal markers (same overlay you showed in the screenshot)
    private readonly By _loginEmailInput = By.CssSelector(
        "input[type='email'], input[name*='email'], input[id*='email']");

    private readonly By _loginPasswordInput = By.CssSelector(
        "input[type='password'], input[name*='password'], input[id*='password']");

    private readonly By _loginDimmerOverlay = By.CssSelector(
        ".ui.page.modals.dimmer.transition.visible.active");

    public LanguagesComponent(IWebDriver driver)
    {
        _driver = driver;
        _actions = new CommonActions(driver);
    }

    public void AddLanguage(string language, string level)
    {
        EnsureLoggedInIfModalVisible();

        EnsureLanguagesSectionVisible();

        // If the language already exists, remove it first so the test
        // is independent of existing state.
        TryDeleteLanguageIfExists(language);

        // Wait for the Add New button in the Languages grid.
        WaitHelper.WaitForElementVisible(_driver, _addNewButton, 20);

        _actions.Click(_addNewButton);
        _actions.Type(_languageInput, language);

        var dropdown = WaitHelper.WaitForElementVisible(_driver, _levelDropdown, 10);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", dropdown);

        var select = new SelectElement(dropdown);
        select.SelectByText(level);

        _actions.Click(_addButton);

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
            d.FindElements(_languageRowsBy)
             .Any(r =>
             {
                 var tds = r.FindElements(By.TagName("td"));
                 return tds.Count > 0 &&
                        tds[0].Text.Trim().Equals(language, StringComparison.OrdinalIgnoreCase);
             }));
    }

    public void UpdateLanguageLevel(string language, string newLevel)
    {
        EnsureLoggedInIfModalVisible();

        EnsureLanguagesSectionVisible();

        WaitHelper.WaitForElementVisible(_driver, _addNewButton, 20);

        var rowLocator = By.XPath("//div[@id='account-profile-section']//table//tr[td[normalize-space()='" + language + "']]");
        var rows = _driver.FindElements(rowLocator);
        if (!rows.Any())
        {
            // If the language row is not present, nothing to update.
            return;
        }

        var row = rows.First();

        row.FindElement(By.CssSelector("i.outline.write.icon, i.edit.icon")).Click();

        var dropdown = WaitHelper.WaitForElementVisible(_driver, _levelDropdown, 10);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", dropdown);

        var select = new SelectElement(dropdown);
        select.SelectByText(newLevel);

        _actions.Click(_updateButton);
    }

    public void DeleteLanguage(string language)
    {
        EnsureLoggedInIfModalVisible();

        EnsureLanguagesSectionVisible();

        WaitHelper.WaitForElementVisible(_driver, _addNewButton, 20);

        var rowLocator = By.XPath("//div[@id='account-profile-section']//table//tr[td[normalize-space()='" + language + "']]");
        var rows = _driver.FindElements(rowLocator);
        if (!rows.Any())
        {
            // If the language row is already gone, treat as deleted.
            return;
        }

        var deleteIcon = rows.First().FindElement(
            By.CssSelector("i.remove.icon, i.trash.icon, i[title='Remove']"));

        deleteIcon.Click();
    }

    public bool HasLanguage(string language)
    {
        OpenLanguageTab();

        return _driver.FindElements(_languageRowsBy)
            .Any(r =>
            {
                var tds = r.FindElements(By.TagName("td"));
                return tds.Count > 0 &&
                       tds[0].Text.Trim().Equals(language, StringComparison.OrdinalIgnoreCase);
            });
    }

    private void TryDeleteLanguageIfExists(string language)
    {
        OpenLanguageTab();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        while (true)
        {
            var rows = _driver.FindElements(_languageRowsBy);
            IWebElement? targetRow = null;

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count > 0 && cells[0].Text.Trim().Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow == null)
            {
                break;
            }

            targetRow.FindElement(By.XPath(".//i[contains(@class,'remove icon')]")).Click();

            wait.Until(d =>
                !d.FindElements(_languageRowsBy)
                 .Any(r =>
                 {
                     var tds = r.FindElements(By.TagName("td"));
                     return tds.Count > 0 &&
                            tds[0].Text.Trim().Equals(language, StringComparison.OrdinalIgnoreCase);
                 }));
        }
    }

    private void OpenLanguageTab()
    {
        EnsureLanguagesSectionVisible();
    }

    private void EnsureLanguagesSectionVisible()
    {
        if (_driver.FindElements(_addNewButton).Any())
        {
            return;
        }

        try
        {
            var possibleTabs = new[]
            {
                By.XPath("//a[contains(normalize-space(),'Languages')]"),
                By.XPath("//*[contains(@class,'item') and contains(normalize-space(),'Languages')]")
            };

            foreach (var locator in possibleTabs)
            {
                var elements = _driver.FindElements(locator);
                if (elements.Any())
                {
                    elements.First().Click();
                    break;
                }
            }
        }
        catch (WebDriverException)
        {
            // Best-effort only; if this fails, subsequent waits will surface the issue.
        }

        try
        {
            WaitHelper.WaitForElementVisible(_driver, _addNewButton, 20);
        }
        catch (WebDriverTimeoutException)
        {
            // Let callers fail explicitly if the section never becomes visible.
        }
    }

    private void EnsureLoggedInIfModalVisible()
    {
        try
        {
            var emailFields = _driver.FindElements(_loginEmailInput);
            var passwordFields = _driver.FindElements(_loginPasswordInput);

            if (emailFields.Any() && passwordFields.Any())
            {
                // Perform login through the modal.
                var username = JsonReader.GetString("login", "username");
                var password = JsonReader.GetString("login", "password");

                var loginPage = new LoginPage(_driver);
                loginPage.Login(username, password);

                // Wait for the dimmer overlay to disappear.
                WaitHelper.WaitForElementInvisible(_driver, _loginDimmerOverlay, 10);
            }
        }
        catch (WebDriverException)
        {
            // If we can't detect the modal reliably, continue; actions will fail visibly if truly blocked.
        }
    }
}


