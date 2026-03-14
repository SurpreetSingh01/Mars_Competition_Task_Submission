using Mars.Part2.UiTests.Components.Common;
using Mars.Part2.UiTests.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Mars.Part2.UiTests.Components.Marketplace;

public sealed class ShareSkillFormComponent : BaseComponent
{
    private static readonly By[] TitleLocators =
    {
        By.CssSelector("input[name='title']"),
        By.Name("title")
    };

    private static readonly By[] DescriptionLocators =
    {
        By.CssSelector("textarea[name='description']"),
        By.Name("description"),
        By.CssSelector("textarea")
    };

    private static readonly By[] CategoryLocators =
    {
        By.CssSelector("select[name='categoryId']"),
        By.Name("categoryId")
    };

    private static readonly By[] SubCategoryLocators =
    {
        By.CssSelector("select[name='subcategoryId']"),
        By.Name("subcategoryId")
    };

    private static readonly By[] TagsLocators =
    {
        By.CssSelector("input.ReactTags__tagInputField"),
        By.CssSelector("input[placeholder='Add new tag']"),
        By.CssSelector(".ReactTags__tagInput input")
    };

    private static readonly By[] StartDateLocators =
    {
        By.CssSelector("input[name='startDate']"),
        By.Name("startDate")
    };

    private static readonly By[] EndDateLocators =
    {
        By.CssSelector("input[name='endDate']"),
        By.Name("endDate")
    };

    private static readonly By[] CreditAmountLocators =
    {
        By.CssSelector("input[name='charge']"),
        By.Name("charge")
    };

    private static readonly By[] SaveButtonLocators =
    {
        By.CssSelector("input.ui.teal.button[value='Save']"),
        By.CssSelector("input[type='button'][value='Save']"),
        By.XPath("//button[contains(@class,'teal') and normalize-space(.)='Save']"),
        By.XPath("//button[contains(.,'Save')]")
    };

    private static readonly By[] ModernAvailabilitySlotLocators =
    {
        By.CssSelector(".fc-agenda-slots td.fc-widget-content"),
        By.CssSelector(".fc-time-grid td.fc-widget-content"),
        By.CssSelector(".k-scheduler-content td"),
        By.XPath("//table[contains(@class,'fc-agenda-slots')]//td[not(contains(@class,'fc-axis')) and not(contains(@class,'fc-widget-header'))]")
    };

    private static readonly Dictionary<string, string> ServiceTypeChoices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Hourly"] = "Hourly basis service",
        ["Hourly basis service"] = "Hourly basis service",
        ["OneOff"] = "One-off service",
        ["One Off"] = "One-off service",
        ["One-off"] = "One-off service",
        ["One-off service"] = "One-off service"
    };

    private static readonly Dictionary<string, string> LocationTypeChoices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Online"] = "Online",
        ["Onsite"] = "On-site",
        ["On site"] = "On-site",
        ["On-site"] = "On-site"
    };

    private static readonly Dictionary<string, string> SkillTradeChoices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Credit"] = "Credit",
        ["SkillExchange"] = "Skill-exchange",
        ["Skill Exchange"] = "Skill-exchange",
        ["Skill-exchange"] = "Skill-exchange"
    };

    private static readonly Dictionary<string, string> ActiveStatusChoices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Active"] = "Active",
        ["Hidden"] = "Hidden"
    };

    public ShareSkillFormComponent(UiTestContext context) : base(context)
    {
    }

    public void Fill(ShareSkillData data)
    {
        Element.EnterText("share skill title", data.Title, TitleLocators);
        Element.EnterText("share skill description", data.Description, DescriptionLocators);
        Element.SelectDropdownByText("share skill category", data.Category, CategoryLocators);
        SelectSubCategory(data.SubCategory);

        AddTags(data.Tags);
        EnsureAtLeastOneTag();

        SelectMappedChoice(data.ServiceType, ServiceTypeChoices);
        SelectMappedChoice(data.LocationType, LocationTypeChoices);
        SelectMappedChoice(data.ActiveStatus, ActiveStatusChoices);

        EnsureSkillTradeSelection(data.SkillTradeType);
        FillSkillExchangeTextIfVisible(data.SkillTradeType);

        SetDateValue("share skill start date", ResolveStartDate(data.StartDate), StartDateLocators);
        SetDateValue("share skill end date", ResolveEndDate(data.EndDate), EndDateLocators);

        PopulateAvailability(data);

        if (string.Equals(data.SkillTradeType, "Credit", StringComparison.OrdinalIgnoreCase) && Element.ExistsAny(CreditAmountLocators))
        {
            var amount = string.IsNullOrWhiteSpace(data.CreditAmount) ? "5" : data.CreditAmount;
            Element.EnterText("share skill credit amount", amount, CreditAmountLocators);
        }

        FillVisibleRequiredFields();
    }

    public void Submit()
    {
        Element.Click("share skill save button", SaveButtonLocators);
        Wait.WaitForDocumentReady();

        var navigated = Wait.TryWaitForCondition(driver =>
            driver.Url.Contains("/Home/ListingManagement", StringComparison.OrdinalIgnoreCase),
            5);

        if (navigated)
        {
            return;
        }

        var pageText = Driver.FindElement(By.TagName("body")).Text;
        if (pageText.Contains("Please complete", StringComparison.OrdinalIgnoreCase) ||
            pageText.Contains("complete the form", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Share Skill form was not saved because required fields are still missing on the page.");
        }
    }

    private void AddTags(string tagsCsv)
    {
        var tags = tagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var tag in tags)
        {
            Element.AddTag("share skill tags", tag, TagsLocators);
        }
    }

    private void EnsureAtLeastOneTag()
    {
        var hasTag = Driver.FindElements(By.CssSelector(".ReactTags__selected span, .ReactTags__tag")).Any(e => e.Displayed);
        if (!hasTag)
        {
            Element.AddTag("share skill tags", "automation", TagsLocators);
        }
    }

    private void PopulateAvailability(ShareSkillData data)
    {
        var dayIndex = string.IsNullOrWhiteSpace(data.AvailableDayIndex) ? "1" : data.AvailableDayIndex;
        if (HasLegacyAvailabilityControls(dayIndex))
        {
            SetAvailabilityDay(dayIndex, true);
            EnterAvailabilityTime("share skill start time", "StartTime", dayIndex, data.StartTime);
            EnterAvailabilityTime("share skill end time", "EndTime", dayIndex, data.EndTime);
            return;
        }

        SelectModernAvailabilitySlot();
    }

    private void SelectModernAvailabilitySlot()
    {
        foreach (var locator in ModernAvailabilitySlotLocators)
        {
            var slot = Driver.FindElements(locator).FirstOrDefault(e => e.Displayed && e.Enabled);
            if (slot is null)
            {
                continue;
            }

            try
            {
                Element.ScrollIntoView(slot);
                slot.Click();
            }
            catch (Exception)
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", slot);
            }

            return;
        }
    }

    private void SetDateValue(string description, string value, params By[] locators)
    {
        if (!Element.ExistsAny(locators))
        {
            return;
        }

        Element.EnterText(description, value, locators);
    }

    private void FillVisibleRequiredFields()
    {
        var required = Driver.FindElements(By.CssSelector("input[required], textarea[required], select[required]"))
            .Where(e => e.Displayed)
            .ToList();

        var handledRadioNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in required)
        {
            var tagName = field.TagName;
            var type = field.GetAttribute("type") ?? string.Empty;
            var name = field.GetAttribute("name") ?? string.Empty;

            if (string.Equals(type, "radio", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(name) && !handledRadioNames.Contains(name))
                {
                    handledRadioNames.Add(name);
                    EnsureRequiredRadioGroupChecked(name);
                }

                continue;
            }

            if (string.Equals(tagName, "select", StringComparison.OrdinalIgnoreCase))
            {
                var select = new SelectElement(field);
                if (!IsSelectableOption(select.SelectedOption.Text, select.SelectedOption.GetAttribute("value")))
                {
                    var option = select.Options.FirstOrDefault(o => IsSelectableOption(o.Text, o.GetAttribute("value")));
                    option?.Click();
                }

                continue;
            }

            var value = field.GetAttribute("value");
            if (!string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (string.Equals(type, "date", StringComparison.OrdinalIgnoreCase))
            {
                var fallback = name.Contains("end", StringComparison.OrdinalIgnoreCase)
                    ? DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
                    : DateTime.Today.ToString("yyyy-MM-dd");
                field.SendKeys(fallback);
            }
            else
            {
                field.SendKeys("automation");
            }
        }
    }

    private void EnsureRequiredRadioGroupChecked(string name)
    {
        var radios = Driver.FindElements(By.CssSelector($"input[type='radio'][name='{name}']")).Where(e => e.Displayed).ToList();
        if (radios.Count == 0 || radios.Any(r => r.Selected))
        {
            return;
        }

        try
        {
            radios[0].Click();
        }
        catch (Exception)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", radios[0]);
        }
    }

    private void EnsureSkillTradeSelection(string requestedValue)
    {
        var uiLabel = SkillTradeChoices.TryGetValue(requestedValue, out var mapped) ? mapped : requestedValue;

        try
        {
            Element.ClickLabelByText(uiLabel);
        }
        catch (Exception)
        {
        }

        var radios = Driver.FindElements(By.XPath("//input[@type='radio' and (@name='skillTrades' or @name='skillTradeType' or @name='skillTrade') ]"))
            .Where(e => e.Displayed)
            .ToList();

        if (radios.Count == 0 || radios.Any(r => r.Selected))
        {
            return;
        }

        var target = radios.FirstOrDefault(radio =>
        {
            var id = radio.GetAttribute("id");
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var label = Driver.FindElements(By.CssSelector($"label[for='{id}']")).FirstOrDefault();
            return label is not null && label.Text.Contains(uiLabel, StringComparison.OrdinalIgnoreCase);
        }) ?? radios[0];

        try
        {
            target.Click();
        }
        catch (Exception)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", target);
        }
    }

    private void FillSkillExchangeTextIfVisible(string skillTradeType)
    {
        if (!string.Equals(skillTradeType, "SkillExchange", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(skillTradeType, "Skill Exchange", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(skillTradeType, "Skill-exchange", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var tagInputs = Driver.FindElements(By.CssSelector("input.ReactTags__tagInputField, .ReactTags__tagInput input, input[placeholder='Add new tag']"))
            .Where(e => e.Displayed)
            .ToList();

        if (tagInputs.Count > 1)
        {
            var exchangeTagInput = tagInputs[^1];
            exchangeTagInput.SendKeys("exchange-skill");
            exchangeTagInput.SendKeys(Keys.Enter);
            return;
        }

        var directField = Driver.FindElements(By.CssSelector("input[name*='skill' i][name*='exchange' i], input[id*='skill' i][id*='exchange' i], input[placeholder*='skill' i][placeholder*='exchange' i]"))
            .FirstOrDefault(e => e.Displayed);

        if (directField is null)
        {
            directField = Driver.FindElements(By.XPath("//label[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'skill-exchange') or contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'skill exchange')]/following::input[1]"))
                .FirstOrDefault(e => e.Displayed);
        }

        if (directField is null)
        {
            return;
        }

        directField.Clear();
        directField.SendKeys("exchange-skill");
    }
    private static string ResolveStartDate(string startDate)
    {
        return string.IsNullOrWhiteSpace(startDate)
            ? DateTime.Today.ToString("yyyy-MM-dd")
            : startDate;
    }

    private static string ResolveEndDate(string endDate)
    {
        return string.IsNullOrWhiteSpace(endDate)
            ? DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
            : endDate;
    }

    private void SetAvailabilityDay(string dayIndex, bool shouldBeChecked)
    {
        var locator = By.XPath($"//input[@name='Available' and @index='{dayIndex}']");
        var checkbox = Element.FindVisible($"availability day checkbox {dayIndex}", locator);
        var isChecked = checkbox.Selected || string.Equals(checkbox.GetAttribute("checked"), "true", StringComparison.OrdinalIgnoreCase);

        if (isChecked == shouldBeChecked)
        {
            return;
        }

        try
        {
            checkbox.Click();
        }
        catch (Exception)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", checkbox);
        }
    }

    private void EnterAvailabilityTime(string description, string fieldName, string dayIndex, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var locator = By.XPath($"//*[@name='{fieldName}' and @index='{dayIndex}']");
        var field = Element.FindVisible(description, locator);

        if (string.Equals(field.TagName, "select", StringComparison.OrdinalIgnoreCase))
        {
            var select = new SelectElement(field);
            var option = select.Options.FirstOrDefault(item =>
                string.Equals(item.Text.Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase) ||
                NormalizeTimeText(item.Text) == NormalizeTimeText(value));
            option?.Click();
            return;
        }

        Element.EnterText(description, value, locator);
    }

    private void SelectMappedChoice(string requestedValue, IReadOnlyDictionary<string, string> knownChoices)
    {
        if (string.IsNullOrWhiteSpace(requestedValue))
        {
            return;
        }

        var uiLabel = knownChoices.TryGetValue(requestedValue, out var mapped) ? mapped : requestedValue;
        Element.ClickLabelByText(uiLabel);
    }

    private void SelectSubCategory(string requestedValue)
    {
        if (!Element.ExistsAny(SubCategoryLocators))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(requestedValue))
        {
            Element.SelectDropdownByText("share skill subcategory", requestedValue, SubCategoryLocators);
            return;
        }

        Wait.WaitForCondition(driver =>
        {
            foreach (var locator in SubCategoryLocators)
            {
                var candidate = driver.FindElements(locator).FirstOrDefault();
                if (candidate is null)
                {
                    continue;
                }

                var select = new SelectElement(candidate);
                if (select.Options.Any(option => IsSelectableOption(option.Text, option.GetAttribute("value"))))
                {
                    return true;
                }
            }

            return false;
        }, "share skill subcategory options to load", 10);

        var subCategorySelect = new SelectElement(Element.FindVisible("share skill subcategory", SubCategoryLocators));
        var firstOption = subCategorySelect.Options.First(option => IsSelectableOption(option.Text, option.GetAttribute("value")));
        firstOption.Click();
    }

    private static bool IsSelectableOption(string text, string? value)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               !text.Contains("Select", StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(value) &&
               !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);
    }

    private bool HasLegacyAvailabilityControls(string dayIndex)
    {
        return Driver.FindElements(By.XPath($"//input[@name='Available' and @index='{dayIndex}']")).Count > 0;
    }

    private static string NormalizeTimeText(string value)
    {
        if (DateTime.TryParse(value, out var parsedTime))
        {
            return parsedTime.ToString("hh:mmtt", System.Globalization.CultureInfo.InvariantCulture);
        }

        return value
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();
    }
}



