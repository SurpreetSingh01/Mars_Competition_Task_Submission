using MarsAutomation.Components;
using MarsAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Pages;

public class ShareSkillPage
{
    private enum SaveAttemptOutcome
    {
        Pending,
        Succeeded,
        ValidationFailed
    }

    private readonly IWebDriver _driver;
    private readonly CommonActions _actions;
    private readonly NavigationMenu _menu;

    private readonly By _titleInput = By.Name("title");
    private readonly By _descriptionInput = By.Name("description");
    private readonly By _categoryDropdown = By.CssSelector(
        "select[name='categoryId'], select[id='categoryId'], select[name*='category'], select[id*='category']");
    private readonly By _tagsInput = By.CssSelector(
        "div.react-tagsinput input[placeholder='Add new tag'], " +
        "div.react-tagsinput input[placeholder*='tag'], " +
        "input[placeholder='Add new tag']");
    private readonly By _saveButton = By.XPath("//input[@value='Save'] | //button[normalize-space()='Save']");
    private readonly By _listingRows = By.CssSelector("table tbody tr");
    private readonly By _manageListingsTab = By.XPath("//*[self::a or self::button][contains(normalize-space(),'Manage Listings')]");
    private readonly By _skillExchangeLabel = By.XPath(
        "//*[starts-with(normalize-space(),'Skill-Exchange')]");
    private readonly By _skillExchangeInput = By.XPath(
        "(//*[starts-with(normalize-space(),'Skill-Exchange')])[1]/following::input[@placeholder='Add new tag'][1]");
    private readonly By _priceInput = By.CssSelector(
        "input[name='charge'], input[name='Charge'], input[id*='charge'], input[id*='Charge'], input[name='amount'], input[id*='amount']");

    private readonly By _validationMessages = By.CssSelector(".validation-summary-errors li, span.field-validation-error, .ui.basic.red.pointing.prompt.label, .ui.red.message, .message.error, .toast-message, .ns-box-inner");
    private readonly By _manageListingsHeader = By.XPath("//*[self::h1 or self::h2 or self::h3][normalize-space()='Manage Listings']");
    private readonly By _shareSkillButton = By.XPath("//*[self::a or self::button][normalize-space()='Share Skill']");
    private readonly By _deleteConfirmationButton = By.XPath(
        "//button[normalize-space()='Yes' or normalize-space()='Delete' or normalize-space()='Confirm' or normalize-space()='OK']" +
        " | //input[@type='button' and (@value='Yes' or @value='Delete' or @value='Confirm' or @value='OK')]" +
        " | //input[@type='submit' and (@value='Yes' or @value='Delete' or @value='Confirm' or @value='OK')]");

    public ShareSkillPage(IWebDriver driver)
    {
        _driver = driver;
        _actions = new CommonActions(driver);
        _menu = new NavigationMenu(driver);
    }

    public void NavigateToShareSkill()
    {
        _menu.GoToShareSkill();
        EnsurePageReady();
    }

    public void CreateListing(string title, string description, string category, string tag)
    {
        EnsurePageReady();

        _actions.Type(_titleInput, title);
        _actions.Type(_descriptionInput, description);

        SelectCategory(category);
        SelectFirstAvailableSubCategory();

        AddTag(tag);
        ConfigureServiceDetails();
        FillSkillExchangeTagIfVisible(tag);
        CompleteRemainingFormFields(title, description, tag);

        _actions.Click(_saveButton);
        RetrySaveIfValidationShown(title, description, tag);

        WaitHelper.WaitUntil(_driver, _ => IsListingVisible(title), 20);
    }

    public void SubmitEmptyForm()
    {
        EnsurePageReady();
        _actions.Click(_saveButton);
    }

    public IReadOnlyCollection<IWebElement> GetValidationMessages()
    {
        try
        {
            WaitHelper.WaitUntil(_driver, _ =>
                FindVisibleValidationElements().Any() ||
                GetRequiredFields().Any(field => HasFieldValidation(field)),
                5);
        }
        catch
        {
            // Fall through to the field-level checks below.
        }

        var messages = FindVisibleValidationElements();

        if (messages.Any())
            return messages;

        var invalidFields = GetRequiredFields()
            .Where(HasFieldValidation)
            .ToList();

        return invalidFields;
    }

    public void DeleteListing(string title)
    {
        OpenManageListings();

        var rowLocator = By.XPath($"//table//tr[.//td[contains(normalize-space(),\"{title}\")]]");

        WaitHelper.WaitUntil(_driver, driver =>
            driver.FindElements(rowLocator).Any(row => row.Displayed),
            60);

        var row = _driver.FindElements(rowLocator).First(row => row.Displayed);

        var deleteTarget = FindDeleteTarget(row);
        if (deleteTarget == null)
        {
            throw new NoSuchElementException($"Could not find a delete control for listing '{title}'.");
        }

        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", deleteTarget);

        try
        {
            deleteTarget.Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", deleteTarget);
        }

        ConfirmDeleteIfPrompted();

        WaitForListingToDisappear(rowLocator);
    }

    private void EnsurePageReady()
    {
        var loginPage = new LoginPage(_driver);
        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        loginPage.LoginIfNeeded(username, password);
        loginPage.DismissModalIfPresent();
        WaitHelper.WaitForElementVisible(_driver, _saveButton, 10);
    }

    private void SelectCategory(string category)
    {
        var dropdownElement = FindVisibleCategoryDropdown();
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", dropdownElement);

        SelectFirstMatchingOption(dropdownElement, category);
    }

    private void SelectFirstAvailableSubCategory()
    {
        var subCategorySelects = _driver.FindElements(By.CssSelector(
                "select[name='subcategoryId'], select[id='subcategoryId'], select[name*='subcategory'], select[id*='subcategory']"))
            .Where(IsVisibleAndEnabled)
            .ToList();
        if (!subCategorySelects.Any())
            return;

        SelectFirstNonPlaceholderOption(subCategorySelects.First());
    }

    private IWebElement FindVisibleCategoryDropdown()
    {
        var candidates = _driver.FindElements(_categoryDropdown)
            .Where(IsVisibleAndEnabled)
            .ToList();

        if (candidates.Any())
        {
            return candidates.First();
        }

        return WaitHelper.WaitForElementVisible(_driver,
            By.XPath("//label[contains(normalize-space(),'Category')]/following::select[1]"),
            10);
    }

    private void SelectFirstMatchingOption(IWebElement selectElement, string desiredText)
    {
        var select = new SelectElement(selectElement);

        var exactOption = select.Options.FirstOrDefault(option =>
            IsRealOption(option) &&
            option.Text.Trim().Equals(desiredText, StringComparison.OrdinalIgnoreCase));

        if (exactOption != null)
        {
            SetSelectOption(selectElement, exactOption);
            return;
        }

        var partialOption = select.Options.FirstOrDefault(option =>
            IsRealOption(option) &&
            option.Text.Contains(desiredText, StringComparison.OrdinalIgnoreCase));

        if (partialOption != null)
        {
            SetSelectOption(selectElement, partialOption);
            return;
        }

        SelectFirstNonPlaceholderOption(selectElement);
    }

    private void SelectFirstNonPlaceholderOption(IWebElement selectElement)
    {
        var select = new SelectElement(selectElement);
        var option = select.Options.FirstOrDefault(IsRealOption);

        if (option == null)
            throw new NoSuchElementException("Could not find a selectable category option in the Share Skill form.");

        SetSelectOption(selectElement, option);
    }

    private IEnumerable<IWebElement> GetRequiredFields()
    {
        return new[]
        {
            _driver.FindElements(_titleInput).FirstOrDefault(IsVisible),
            _driver.FindElements(_descriptionInput).FirstOrDefault(IsVisible),
            _driver.FindElements(_categoryDropdown).FirstOrDefault(IsVisible)
        }.Where(e => e != null)!;
    }

    private string GetValidationMessage(IWebElement element)
    {
        try
        {
            return (string)((IJavaScriptExecutor)_driver).ExecuteScript(
                "return arguments[0].validationMessage || '';",
                element)!;
        }
        catch (StaleElementReferenceException)
        {
            return string.Empty;
        }
    }

    private void ConfigureServiceDetails()
    {
        ClickIfPresent(
            By.XPath("//label[contains(.,'Mon')]/preceding-sibling::input[@type='checkbox'] | //input[@type='checkbox' and contains(@name,'Available')]"));
        ClickIfPresent(
            By.XPath("//input[@name='serviceType' and @value='1'] | //label[contains(.,'Hourly basis service')]"));
        ClickIfPresent(
            By.XPath("//input[@name='locationType' and @value='1'] | //label[contains(.,'On-site')]"));
        ClickIfPresent(
            By.XPath("//input[@name='skillTrades' and @value='true'] | //label[contains(.,'Skill-exchange')]"));

        ClickIfPresent(
            By.XPath("//input[@name='active' and @value='true'] | //label[contains(.,'Active')]"));
    }

    private void ClickIfPresent(By locator)
    {
        var elements = _driver.FindElements(locator).Where(e => e.Displayed).ToList();
        if (!elements.Any())
            return;

        var element = elements.First();
        var tagName = (element.TagName ?? string.Empty).ToLowerInvariant();

        try
        {
            if (tagName == "input")
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
            }
            else
            {
                element.Click();
            }
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    private void CompleteRemainingFormFields(string title, string description, string tag)
    {
        var scope = FindFormScope();
        FillEmptyTextInputs(scope, title, tag);
        FillEmptyTextAreas(scope, description);
        FillEmptyDateInputs(scope);
        FillEmptyNumberInputs(scope);
        SelectRemainingDropdowns(scope);
        SelectUnselectedRadioGroups(scope);
        ClickVisibleCheckboxes(scope);
    }

    private IWebElement FindFormScope()
    {
        var save = WaitHelper.WaitForElementVisible(_driver, _saveButton, 10);

        return save.FindElements(By.XPath("./ancestor::form")).FirstOrDefault()
            ?? save.FindElements(By.XPath("./ancestor::div[contains(@class,'ui') or contains(@class,'segment')][1]")).FirstOrDefault()
            ?? _driver.FindElement(By.TagName("body"));
    }

    private void FillEmptyTextInputs(IWebElement scope, string title, string tag)
    {
        var inputs = scope.FindElements(By.XPath(".//input[not(@type='hidden') and not(@type='file') and not(@type='checkbox') and not(@type='radio') and not(@type='submit') and not(@type='button')]"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        foreach (var input in inputs)
        {
            var name = (input.GetAttribute("name") ?? string.Empty).ToLowerInvariant();
            var placeholder = (input.GetAttribute("placeholder") ?? string.Empty).ToLowerInvariant();
            var currentValue = input.GetAttribute("value") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(currentValue))
                continue;

            if (name.Contains("title"))
                continue;

            if (placeholder.Contains("tag"))
                continue;

            if (placeholder.Contains("skill"))
            {
                input.SendKeys(tag + Keys.Enter);
                continue;
            }

            input.SendKeys(title);
        }
    }

    private void FillEmptyTextAreas(IWebElement scope, string description)
    {
        var textAreas = scope.FindElements(By.XPath(".//textarea"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        foreach (var textArea in textAreas)
        {
            if (!string.IsNullOrWhiteSpace(textArea.GetAttribute("value")) || !string.IsNullOrWhiteSpace(textArea.Text))
                continue;

            textArea.SendKeys(description);
        }
    }

    private void FillEmptyDateInputs(IWebElement scope)
    {
        var dateInputs = scope.FindElements(By.XPath(".//input[@type='date' or contains(@name,'Date') or contains(@id,'Date')]"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        if (!dateInputs.Any())
            return;

        var today = DateTime.Today;

        for (var i = 0; i < dateInputs.Count; i++)
        {
            var input = dateInputs[i];
            if (!string.IsNullOrWhiteSpace(input.GetAttribute("value")))
                continue;

            var dateValue = (i == 0 ? today.AddDays(1) : today.AddDays(7)).ToString("yyyy-MM-dd");
            input.SendKeys(dateValue);
        }
    }

    private void FillEmptyNumberInputs(IWebElement scope)
    {
        var numberInputs = scope.FindElements(By.XPath(".//input[@type='number' or contains(@name,'Amount') or contains(@name,'Charge') or contains(@id,'Amount') or contains(@id,'Charge')]"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        foreach (var input in numberInputs)
        {
            if (!string.IsNullOrWhiteSpace(input.GetAttribute("value")))
                continue;

            input.SendKeys("10");
        }
    }

    private void SelectRemainingDropdowns(IWebElement scope)
    {
        var selects = scope.FindElements(By.XPath(".//select"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        foreach (var element in selects)
        {
            var select = new SelectElement(element);
            var selected = select.SelectedOption;
            var selectedValue = selected?.GetAttribute("value") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(selectedValue) && selectedValue != "0")
                continue;

            var option = select.Options.FirstOrDefault(opt =>
                opt.GetAttribute("value") != "0" &&
                !string.IsNullOrWhiteSpace(opt.Text));

            if (option != null)
            {
                SetSelectOption(element, option);
            }
        }
    }

    private void SelectUnselectedRadioGroups(IWebElement scope)
    {
        var radios = scope.FindElements(By.XPath(".//input[@type='radio']"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        var radioGroups = radios
            .GroupBy(r => r.GetAttribute("name") ?? Guid.NewGuid().ToString())
            .Where(g => !g.Any(r => r.Selected));

        foreach (var group in radioGroups)
        {
            var choice = group.FirstOrDefault();
            if (choice == null)
                continue;

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", choice);
        }
    }

    private void ClickVisibleCheckboxes(IWebElement scope)
    {
        var checkboxes = scope.FindElements(By.XPath(".//input[@type='checkbox']"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        foreach (var checkbox in checkboxes)
        {
            if (checkbox.Selected)
                continue;

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", checkbox);
        }
    }

    private void RetrySaveIfValidationShown(string title, string description, string tag)
    {
        var firstAttemptOutcome = WaitForSaveAttemptOutcome(title, 20);
        if (firstAttemptOutcome == SaveAttemptOutcome.Succeeded)
            return;

        if (firstAttemptOutcome != SaveAttemptOutcome.ValidationFailed)
            throw new WebDriverTimeoutException("Save did not complete or surface validation errors within the expected time.");

        if (!IsShareSkillFormVisible())
            return;

        FillSkillExchangeTagIfVisible(tag);
        CompleteRemainingFormFields(title, description, tag);
        _actions.Click(_saveButton);

        var secondAttemptOutcome = WaitForSaveAttemptOutcome(title, 20);
        if (secondAttemptOutcome == SaveAttemptOutcome.Succeeded)
            return;

        var validationSummary = string.Join("; ", GetVisibleValidationTexts());
        throw new WebDriverException(
            $"Save still failed after retry. Validation messages: {(string.IsNullOrWhiteSpace(validationSummary) ? "none captured" : validationSummary)}");
    }

    private static bool IsVisibleAndEnabled(IWebElement element)
    {
        try
        {
            return element.Displayed && element.Enabled;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsRealOption(IWebElement option)
    {
        var value = option.GetAttribute("value") ?? string.Empty;
        var text = option.Text.Trim();

        return !string.IsNullOrWhiteSpace(text) &&
               value != "0" &&
               !text.Equals("Select Category", StringComparison.OrdinalIgnoreCase) &&
               !text.StartsWith("Select ", StringComparison.OrdinalIgnoreCase);
    }

    private void SetSelectOption(IWebElement selectElement, IWebElement optionElement)
    {
        var value = optionElement.GetAttribute("value") ?? string.Empty;

        ((IJavaScriptExecutor)_driver).ExecuteScript(@"
            const select = arguments[0];
            const value = arguments[1];
            select.value = value;
            select.dispatchEvent(new Event('change', { bubbles: true }));
            select.dispatchEvent(new Event('input', { bubbles: true }));
        ", selectElement, value);

        var selectedValue = selectElement.GetAttribute("value") ?? string.Empty;
        if (!string.Equals(selectedValue, value, StringComparison.OrdinalIgnoreCase))
        {
            new SelectElement(selectElement).SelectByValue(value);
        }
    }

    private void AddTag(string tag)
    {
        var scope = FindFormScope();
        var tagInput = scope.FindElements(By.CssSelector(
                "div.react-tagsinput input[placeholder='Add new tag'], " +
                "div.react-tagsinput input[placeholder*='tag'], " +
                "input[placeholder='Add new tag']"))
            .Where(IsVisibleAndEnabled)
            .FirstOrDefault()
            ?? _driver.FindElements(_tagsInput).Where(IsVisibleAndEnabled).FirstOrDefault();

        if (tagInput == null)
        {
            throw new NoSuchElementException("Could not find the visible Tags input in the Share Skill form.");
        }

        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", tagInput);
        tagInput.Clear();
        tagInput.SendKeys(tag);
        tagInput.SendKeys(Keys.Enter);
        tagInput.SendKeys(Keys.Tab);

        WaitHelper.WaitUntil(_driver, _ =>
        {
            var scopeText = scope.Text ?? string.Empty;
            return scopeText.Contains(tag, StringComparison.OrdinalIgnoreCase);
        }, 5);
    }

    private IReadOnlyCollection<IWebElement> FindVisibleValidationElements()
    {
        var selectorMatches = _driver.FindElements(_validationMessages)
            .Where(HasVisibleText)
            .ToList();

        if (selectorMatches.Any())
            return selectorMatches;

        return _driver.FindElements(By.XPath(
                "//*[contains(translate(normalize-space(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'required')" +
                " or contains(normalize-space(), 'Please complete the form correctly')]"))
            .Where(HasVisibleText)
            .ToList();
    }

    private IEnumerable<string> GetVisibleValidationTexts()
    {
        return FindVisibleValidationElements()
            .Select(TryGetText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim());
    }

    private bool HasFieldValidation(IWebElement field)
    {
        return !string.IsNullOrWhiteSpace(GetValidationMessage(field)) || HasInvalidState(field);
    }

    private bool HasInvalidState(IWebElement field)
    {
        try
        {
            var cssClass = field.GetAttribute("class") ?? string.Empty;
            var ariaInvalid = field.GetAttribute("aria-invalid") ?? string.Empty;

            return ariaInvalid.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   cssClass.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                   cssClass.Contains("invalid", StringComparison.OrdinalIgnoreCase);
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private SaveAttemptOutcome WaitForSaveAttemptOutcome(string title, int timeoutSeconds = 10)
    {
        SaveAttemptOutcome outcome = SaveAttemptOutcome.Pending;

        try
        {
            WaitHelper.WaitUntil(_driver, _ =>
            {
                outcome = GetSaveAttemptOutcome(title);
                return outcome != SaveAttemptOutcome.Pending;
            }, timeoutSeconds);
        }
        catch (WebDriverTimeoutException)
        {
            outcome = GetSaveAttemptOutcome(title);
        }

        return outcome;
    }

    private SaveAttemptOutcome GetSaveAttemptOutcome(string title)
    {
        if (IsListingVisible(title) || IsManageListingsPage())
            return SaveAttemptOutcome.Succeeded;

        if (!IsShareSkillFormVisible())
            return SaveAttemptOutcome.Pending;

        return GetVisibleValidationTexts().Any()
            ? SaveAttemptOutcome.ValidationFailed
            : SaveAttemptOutcome.Pending;
    }

    private bool IsManageListingsPage()
    {
        try
        {
            var url = _driver.Url ?? string.Empty;
            if (url.Contains("ManageListing", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("ManageListings", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return _driver.FindElements(_shareSkillButton).Any(IsVisibleAndEnabled) ||
                   _driver.FindElements(_listingRows).Any(IsVisible) ||
                   _driver.FindElements(_manageListingsHeader).Any(IsVisible);
        }
        catch
        {
            return false;
        }
    }

    private bool IsListingVisible(string title)
    {
        try
        {
            var rowLocator = By.XPath($"//table//tr[.//td[contains(normalize-space(),\"{title}\")]]");
            return _driver.FindElements(rowLocator).Any(IsVisible);
        }
        catch
        {
            return false;
        }
    }

    private bool IsShareSkillFormVisible()
    {
        try
        {
            return _driver.FindElements(_saveButton).Any(IsVisibleAndEnabled) &&
                   _driver.FindElements(_titleInput).Any(IsVisible);
        }
        catch
        {
            return false;
        }
    }

    private IWebElement? FindDeleteTarget(IWebElement row)
    {
        var candidates = row.FindElements(By.XPath(
                ".//*[self::button or self::a][.//i[contains(@class,'remove') or contains(@class,'delete') or contains(@class,'close') or contains(@class,'times')]]" +
                " | .//i[contains(@class,'remove') or contains(@class,'delete') or contains(@class,'close') or contains(@class,'times')]/ancestor::*[self::button or self::a][1]" +
                " | .//i[contains(@class,'remove') or contains(@class,'delete') or contains(@class,'close') or contains(@class,'times')]"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private void WaitForListingToDisappear(By rowLocator)
    {
        try
        {
            WaitHelper.WaitUntil(_driver, driver =>
                !driver.FindElements(rowLocator).Any(IsVisible),
                8);
        }
        catch (WebDriverTimeoutException)
        {
            var retryRow = _driver.FindElements(rowLocator).FirstOrDefault(IsVisible);
            if (retryRow == null)
                return;

            var retryDeleteTarget = FindDeleteTarget(retryRow);
            if (retryDeleteTarget == null)
                throw;

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", retryDeleteTarget);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", retryDeleteTarget);
            ConfirmDeleteIfPrompted();

            WaitHelper.WaitUntil(_driver, driver =>
                !driver.FindElements(rowLocator).Any(IsVisible),
                15);
        }
    }

    private void ConfirmDeleteIfPrompted()
    {
        try
        {
            WaitHelper.WaitUntil(_driver, _ =>
            {
                try
                {
                    _driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            }, 2);

            _driver.SwitchTo().Alert().Accept();
            return;
        }
        catch (WebDriverTimeoutException)
        {
            // No browser alert was shown; fall through to in-page confirmation.
        }

        var confirmButton = _driver.FindElements(_deleteConfirmationButton)
            .FirstOrDefault(IsVisibleAndEnabled);

        if (confirmButton == null)
            return;

        try
        {
            confirmButton.Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", confirmButton);
        }
    }

    private static bool HasVisibleText(IWebElement element)
    {
        try
        {
            return element.Displayed && !string.IsNullOrWhiteSpace(element.Text);
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private static bool IsVisible(IWebElement element)
    {
        try
        {
            return element.Displayed;
        }
        catch
        {
            return false;
        }
    }

    private static string TryGetText(IWebElement element)
    {
        try
        {
            return element.Text ?? string.Empty;
        }
        catch (StaleElementReferenceException)
        {
            return string.Empty;
        }
    }

    private void OpenManageListings()
    {
        var tabs = _driver.FindElements(_manageListingsTab).Where(e => e.Displayed).ToList();
        if (!tabs.Any())
            return;

        try
        {
            tabs.First().Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", tabs.First());
        }
    }

    private void FillSkillExchangeTagIfVisible(string tag)
    {
        var labels = _driver.FindElements(_skillExchangeLabel)
            .Where(e => e.Displayed)
            .ToList();

        if (labels.Any())
        {
            var inputs = _driver.FindElements(_skillExchangeInput)
                .Where(IsVisibleAndEnabled)
                .ToList();

            if (inputs.Any())
            {
                var input = inputs.First();
                CommitTag(input, tag);
                WaitHelper.WaitUntil(_driver, _ =>
                {
                    var currentValue = input.GetAttribute("value") ?? string.Empty;
                    var pageText = _driver.PageSource ?? string.Empty;
                    return string.IsNullOrWhiteSpace(currentValue) ||
                           pageText.Contains(tag, StringComparison.OrdinalIgnoreCase);
                }, 5);
                return;
            }
        }

        var allVisibleTagInputs = _driver.FindElements(By.CssSelector("div.react-tagsinput input[placeholder='Add new tag'], input[placeholder='Add new tag']"))
            .Where(IsVisibleAndEnabled)
            .ToList();

        if (allVisibleTagInputs.Count > 1)
        {
            CommitTag(allVisibleTagInputs.Last(), tag);
        }
    }

    private void CommitTag(IWebElement input, string tag)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", input);

        try
        {
            input.Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", input);
        }

        try
        {
            input.Clear();
        }
        catch
        {
            // Some tag inputs do not support Clear; keep going.
        }

        new Actions(_driver)
            .MoveToElement(input)
            .Click()
            .SendKeys(tag)
            .Pause(TimeSpan.FromMilliseconds(200))
            .SendKeys(Keys.Enter)
            .Pause(TimeSpan.FromMilliseconds(200))
            .Perform();

        var currentValue = input.GetAttribute("value") ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            input.SendKeys(Keys.Tab);
        }
    }
}

