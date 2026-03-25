using MarsAutomation.Components;
using MarsAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Pages;

public class SkillsPage
{
    private readonly IWebDriver _driver;
    private readonly CommonActions _actions;

    private readonly By _skillsTab = By.XPath(
        "//a[contains(@data-tab,'second') and contains(normalize-space(),'Skills')]"
        + " | //a[contains(normalize-space(),'Skills')]");
    private readonly By _skillsSection = By.XPath(
        "//div[@data-tab='second' and contains(@class,'active')]"
        + " | //div[contains(@class,'active tab') and @data-tab='second']");
    private readonly By _addNewButton = By.XPath(
        "(//div[@data-tab='second' and contains(@class,'active')]"
        + "//*[self::button or self::input or self::a or self::div]"
        + "[translate(normalize-space(.),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='ADD NEW'])[1]"
        + " | (//div[contains(@class,'active tab') and @data-tab='second']"
        + "//*[self::button or self::input or self::a or self::div]"
        + "[translate(normalize-space(.),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='ADD NEW'])[1]");
    private readonly By _skillInput = By.XPath(
        "(//div[@data-tab='second' and contains(@class,'active')]//input[@name='name'])[1]"
        + " | (//div[contains(@class,'active tab') and @data-tab='second']//input[@name='name'])[1]");
    private readonly By _levelDropdown = By.XPath(
        "(//div[@data-tab='second' and contains(@class,'active')]//select[@name='level'])[1]"
        + " | (//div[contains(@class,'active tab') and @data-tab='second']//select[@name='level'])[1]");
    private readonly By _addButton = By.XPath(
        "(//div[@data-tab='second' and contains(@class,'active')]//input[contains(@class,'button') and @value='Add'])[1]"
        + " | (//div[contains(@class,'active tab') and @data-tab='second']//input[contains(@class,'button') and @value='Add'])[1]");
    private readonly By _updateButton = By.XPath(
        "(//div[@data-tab='second' and contains(@class,'active')]//input[contains(@class,'button') and @value='Update'])[1]"
        + " | (//div[contains(@class,'active tab') and @data-tab='second']//input[contains(@class,'button') and @value='Update'])[1]");

    private readonly By _activeOverlay = By.CssSelector(".ui.page.modals.dimmer.visible.active, .ui.dimmer.visible.active");
    private readonly By _visibleModal = By.CssSelector(".ui.modal.visible.active");
    private readonly By _loginEmailInput = By.CssSelector("input[type='email'], input[name*='email'], input[id*='email']");
    private readonly By _loginPasswordInput = By.CssSelector("input[type='password'], input[name*='password'], input[id*='password']");

    public SkillsPage(IWebDriver driver)
    {
        _driver = driver;
        _actions = new CommonActions(driver);
    }

    public void AddSkill(string skill, string level)
    {
        EnsureSkillsTabReady();
        WaitForPageToBeClear();
        SafeClick(_addNewButton);

        WaitForPageToBeClear();
        _actions.Type(_skillInput, skill);

        WaitForPageToBeClear();
        SelectDropdownByText(_levelDropdown, level);

        WaitForPageToBeClear();
        SafeClick(_addButton);

        WaitForPageToBeClear();
    }

    public void UpdateSkillLevel(string skill, string newLevel)
    {
        EnsureSkillsTabReady();
        WaitForPageToBeClear();

        var row = WaitHelper.WaitForElementVisible(_driver,
            By.XPath($"//div[@data-tab='second']//td[normalize-space()='{skill}']/parent::tr | //div[contains(@class,'active tab') and @data-tab='second']//td[normalize-space()='{skill}']/parent::tr"));

        try
        {
            row.FindElement(By.CssSelector("i.outline.write.icon, i.edit.icon")).Click();
        }
        catch (ElementClickInterceptedException)
        {
            DismissOverlayIfPresent();
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].click();",
                row.FindElement(By.CssSelector("i.outline.write.icon, i.edit.icon")));
        }

        WaitForPageToBeClear();
        SelectDropdownByText(_levelDropdown, newLevel);

        WaitForPageToBeClear();
        SafeClick(_updateButton);

        WaitForPageToBeClear();
    }

    public void DeleteSkill(string skill)
    {
        EnsureSkillsTabReady();
        WaitForPageToBeClear();

        var deleteIcon = WaitHelper.WaitForElementClickable(_driver,
            By.XPath($"//div[@data-tab='second']//td[normalize-space()='{skill}']/parent::tr//i[contains(@class,'remove icon')] | //div[contains(@class,'active tab') and @data-tab='second']//td[normalize-space()='{skill}']/parent::tr//i[contains(@class,'remove icon')]"));

        try
        {
            deleteIcon.Click();
        }
        catch (ElementClickInterceptedException)
        {
            DismissOverlayIfPresent();
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", deleteIcon);
        }

        WaitForPageToBeClear();
    }

    private void EnsureSkillsTabReady()
    {
        EnsureLoggedInIfModalVisible();
        WaitForPageToBeClear();

        var tab = WaitHelper.WaitForElementClickable(_driver, _skillsTab, 20);
        var tabClasses = tab.GetAttribute("class") ?? string.Empty;
        if (!tabClasses.Contains("active", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                tab.Click();
            }
            catch (ElementClickInterceptedException)
            {
                DismissOverlayIfPresent();
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", tab);
            }
        }

        WaitHelper.WaitForElementVisible(_driver, _skillsSection, 20);
        WaitHelper.WaitForElementVisible(_driver, _addNewButton, 20);
    }

    private void SafeClick(By locator)
    {
        WaitForPageToBeClear();

        try
        {
            WaitHelper.WaitForElementClickable(_driver, locator).Click();
        }
        catch (ElementClickInterceptedException)
        {
            DismissOverlayIfPresent();
            var element = WaitHelper.WaitForElementClickable(_driver, locator);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    private void SelectDropdownByText(By locator, string text)
    {
        WaitForPageToBeClear();

        try
        {
            var dropdown = WaitHelper.WaitForElementClickable(_driver, locator);
            var select = new SelectElement(dropdown);
            select.SelectByText(text);
        }
        catch (ElementClickInterceptedException)
        {
            DismissOverlayIfPresent();
            var dropdown = WaitHelper.WaitForElementClickable(_driver, locator);
            var select = new SelectElement(dropdown);
            select.SelectByText(text);
        }
    }

    private void WaitForPageToBeClear()
    {
        DismissOverlayIfPresent();

        WaitHelper.WaitUntil(_driver, driver =>
        {
            var overlays = driver.FindElements(_activeOverlay).Where(e => e.Displayed).ToList();
            var modals = driver.FindElements(_visibleModal).Where(e => e.Displayed).ToList();
            return overlays.Count == 0 && modals.Count == 0;
        }, 10);
    }

    private void EnsureLoggedInIfModalVisible()
    {
        try
        {
            var emailFields = _driver.FindElements(_loginEmailInput);
            var passwordFields = _driver.FindElements(_loginPasswordInput);

            if (!emailFields.Any() || !passwordFields.Any())
                return;

            var username = JsonReader.GetString("login", "username");
            var password = JsonReader.GetString("login", "password");

            var loginPage = new LoginPage(_driver);
            loginPage.Login(username, password);

            WaitHelper.WaitForElementInvisible(_driver, _activeOverlay, 10);
            WaitForPageToBeClear();
        }
        catch (WebDriverException)
        {
            // Continue and let the next explicit wait surface any real issue.
        }
    }

    private void DismissOverlayIfPresent()
    {
        try
        {
            var overlays = _driver.FindElements(_activeOverlay).Where(e => e.Displayed).ToList();
            var modals = _driver.FindElements(_visibleModal).Where(e => e.Displayed).ToList();

            if (!overlays.Any() && !modals.Any())
                return;

            try
            {
                new Actions(_driver).SendKeys(Keys.Escape).Perform();
                Thread.Sleep(200);
            }
            catch { }

            overlays = _driver.FindElements(_activeOverlay).Where(e => e.Displayed).ToList();
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
                catch { }
            }

            overlays = _driver.FindElements(_activeOverlay).Where(e => e.Displayed).ToList();
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
            // swallow cleanup errors
        }
    }
}

