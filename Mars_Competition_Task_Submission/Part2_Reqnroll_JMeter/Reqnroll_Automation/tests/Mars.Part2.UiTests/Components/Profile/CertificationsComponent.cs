using Mars.Part2.UiTests.Components.Common;
using Mars.Part2.UiTests.Models;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Components.Profile;

public sealed class CertificationsComponent : BaseComponent
{
    private static readonly By[] TabLocators =
    {
        By.CssSelector("a[data-tab='fourth']"),
        By.XPath("//a[contains(normalize-space(.),'Certifications')]")
    };

    private static readonly By[] AddButtonLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'fourth')]//div[contains(@class,'teal') and contains(@class,'button') and contains(.,'Add New')]"),
        By.XPath("//div[contains(@data-tab,'fourth')]//div[contains(@class,'teal') and contains(@class,'button') and contains(.,'Add New')]"),
        By.XPath("//div[contains(@class,'button') and contains(.,'Add New')]")
    };

    private static readonly By[] CertificateLocators =
    {
        By.Name("certificationName"),
        By.CssSelector("input[name='certificationName']"),
        By.CssSelector("input[name*='certification' i]")
    };

    private static readonly By[] FromLocators =
    {
        By.Name("certificationFrom"),
        By.CssSelector("input[name='certificationFrom']"),
        By.CssSelector("input[name*='from' i]")
    };

    private static readonly By[] YearLocators =
    {
        By.Name("certificationYear"),
        By.CssSelector("select[name='certificationYear']"),
        By.CssSelector("select[name*='year' i]")
    };

    private static readonly By[] SaveButtonLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'fourth')]//input[@value='Add']"),
        By.XPath("//div[contains(@data-tab,'fourth')]//input[@value='Add']"),
        By.XPath("//div[contains(@data-tab,'fourth')]//button[contains(.,'Add')]"),
        By.XPath("//input[@value='Add']")
    };

    private static readonly By[] RowLocators =
    {
        By.XPath("//div[contains(@class,'active') and contains(@data-tab,'fourth')]//table/tbody/tr"),
        By.XPath("//div[contains(@data-tab,'fourth')]//table/tbody/tr")
    };

    public CertificationsComponent(UiTestContext context) : base(context)
    {
    }

    public void AddCertification(CertificationData data)
    {
        OpenTab();
        Element.Click("certification add new button", AddButtonLocators);
        Element.EnterText("certification name", data.Certificate, CertificateLocators);
        Element.EnterText("certification from", data.From, FromLocators);
        Element.SelectDropdownByText("certification year", data.Year, YearLocators);
        Element.Click("certification save button", SaveButtonLocators);
        Wait.WaitForCondition(_ => HasCertificationInCurrentTab(data), $"certification '{data.Certificate}' to appear", 10);
    }

    public bool HasCertification(CertificationData data)
    {
        OpenTab();
        return HasCertificationInCurrentTab(data);
    }

    public void OpenTab()
    {
        Element.Click("certification tab", TabLocators);
        Wait.WaitForDocumentReady();
    }

    private bool HasCertificationInCurrentTab(CertificationData data)
    {
        var rows = Element.FindAllVisible(RowLocators);
        return rows.Any(row => row.Text.Contains(data.Certificate, StringComparison.OrdinalIgnoreCase) &&
                               row.Text.Contains(data.From, StringComparison.OrdinalIgnoreCase) &&
                               row.Text.Contains(data.Year, StringComparison.OrdinalIgnoreCase));
    }
}
