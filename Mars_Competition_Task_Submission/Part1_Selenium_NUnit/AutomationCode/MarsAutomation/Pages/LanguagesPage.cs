using MarsAutomation.Components;
using MarsAutomation.Helpers;
using OpenQA.Selenium;

namespace MarsAutomation.Pages;

public class LanguagesPage
{
    private readonly IWebDriver _driver;
    private readonly CommonActions _actions;

    // Robust selectors for Languages tab
    private readonly By _addNewButton = By.XPath("//*[self::button or self::input or self::a or self::div][translate(normalize-space(.),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='ADD NEW']");
    private readonly By _languageInput = By.XPath("//input[@name='name']");
    private readonly By _levelDropdown = By.XPath("//select[@name='level']");
    private readonly By _addButton = By.XPath("//input[contains(@class,'button') and @value='Add']");
    private readonly By _updateButton = By.XPath("//input[contains(@class,'button') and @value='Update']");

    public LanguagesPage(IWebDriver driver)
    {
        _driver = driver;
        _actions = new CommonActions(driver);
    }

    public void AddLanguage(string language, string level)
    {
        _actions.Click(_addNewButton);
        _actions.Type(_languageInput, language);
        var dropdown = WaitHelper.WaitForElementClickable(_driver, _levelDropdown);
        dropdown.Click();
        var option = _driver.FindElement(By.XPath($"//select[@name='level']/option[contains(normalize-space(),'{level}')]"));
        option.Click();
        _actions.Click(_addButton);
    }

    public void UpdateLanguageLevel(string language, string newLevel)
    {
        var row = WaitHelper.WaitForElementVisible(_driver,
            By.XPath($"//td[normalize-space()='{language}']/parent::tr"));
        row.FindElement(By.CssSelector("i.outline.write.icon, i.edit.icon")).Click();

        var dropdown = WaitHelper.WaitForElementClickable(_driver, _levelDropdown);
        dropdown.Click();
        _driver.FindElement(By.XPath($"//select[@name='level']/option[contains(normalize-space(),'{newLevel}')]"))
            .Click();
        _actions.Click(_updateButton);
    }

    public void DeleteLanguage(string language)
    {
        var deleteIcon = WaitHelper.WaitForElementClickable(_driver,
            By.XPath($"//td[normalize-space()='{language}']/parent::tr//i[contains(@class,'remove icon')]"));
        deleteIcon.Click();
    }
}

