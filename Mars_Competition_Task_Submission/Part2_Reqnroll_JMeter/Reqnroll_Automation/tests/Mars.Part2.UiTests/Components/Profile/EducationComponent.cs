using Mars.Part2.UiTests.Components.Common;
using Mars.Part2.UiTests.Models;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Components.Profile;

public sealed class EducationComponent : BaseComponent
{
    private static readonly By[] TabLocators =
    {
        By.CssSelector("a[data-tab='third']"),
        By.XPath("//a[contains(normalize-space(.),'Education')]")
    };

    private static readonly By[] AddButtonLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'third')]//div[contains(@class,'teal') and contains(@class,'button') and contains(.,'Add New')]"),
        By.XPath("//div[contains(@data-tab,'third')]//div[contains(@class,'teal') and contains(@class,'button') and contains(.,'Add New')]"),
        By.XPath("//div[contains(@class,'button') and contains(.,'Add New')]")
    };

    private static readonly By[] CountryLocators =
    {
        By.Name("country"),
        By.CssSelector("select[name*='country' i]")
    };

    private static readonly By[] UniversityLocators =
    {
        By.Name("instituteName"),
        By.CssSelector("input[name='instituteName']"),
        By.CssSelector("input[name*='institute' i]"),
        By.CssSelector("input[placeholder*='College' i]")
    };

    private static readonly By[] TitleLocators =
    {
        By.Name("title"),
        By.CssSelector("select[name='title']")
    };

    private static readonly By[] DegreeLocators =
    {
        By.Name("degree"),
        By.CssSelector("input[name='degree']")
    };

    private static readonly By[] GraduationYearLocators =
    {
        By.Name("yearOfGraduation"),
        By.CssSelector("select[name='yearOfGraduation']"),
        By.CssSelector("select[name*='graduation' i]"),
        By.CssSelector("select[name='year']")
    };

    private static readonly By[] SaveButtonLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'third')]//input[@value='Add']"),
        By.XPath("//div[contains(@data-tab,'third')]//input[@value='Add']"),
        By.XPath("//div[contains(@data-tab,'third')]//button[contains(.,'Add')]"),
        By.XPath("//input[@value='Add']")
    };

    private static readonly By[] RowLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'third')]//table/tbody/tr"),
        By.XPath("//div[contains(@data-tab,'third')]//table/tbody/tr")
    };

    public EducationComponent(UiTestContext context) : base(context)
    {
    }

    public void AddEducation(EducationData data)
    {
        OpenTab();
        Element.Click("education add new button", AddButtonLocators);
        Element.SelectDropdownByText("education country", data.Country, CountryLocators);
        Element.EnterText("education university", data.University, UniversityLocators);
        Element.SelectDropdownByText("education title", data.Title, TitleLocators);
        Element.EnterText("education degree", data.Degree, DegreeLocators);
        Element.SelectDropdownByText("education graduation year", data.GraduationYear, GraduationYearLocators);
        Element.Click("education save button", SaveButtonLocators);
        Wait.WaitForCondition(_ => HasEducationRecordInCurrentTab(data), $"education record '{data.University}' to appear", 10);
    }

    public bool HasEducationRecord(EducationData data)
    {
        OpenTab();
        return HasEducationRecordInCurrentTab(data);
    }

    public void OpenTab()
    {
        Element.Click("education tab", TabLocators);
        Wait.WaitForDocumentReady();
    }

    private bool HasEducationRecordInCurrentTab(EducationData data)
    {
        var rows = Element.FindAllVisible(RowLocators);
        var expected = $"{data.University} {data.Country} {data.Title} {data.Degree} {data.GraduationYear}";
        return rows.Any(row => row.Text.Contains(data.University, StringComparison.OrdinalIgnoreCase) &&
                               row.Text.Contains(data.Degree, StringComparison.OrdinalIgnoreCase) &&
                               row.Text.Contains(data.GraduationYear, StringComparison.OrdinalIgnoreCase)) ||
               rows.Any(row => row.Text.Contains(expected, StringComparison.OrdinalIgnoreCase));
    }
}
