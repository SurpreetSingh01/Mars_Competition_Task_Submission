using Mars.Part2.UiTests.Components.Common;
using Mars.Part2.UiTests.Models;

namespace Mars.Part2.UiTests.Pages.Base;

public abstract class BasePage
{
    private readonly LoadingOverlayComponent _loadingOverlay;

    protected BasePage(UiTestContext context)
    {
        Context = context;
        _loadingOverlay = new LoadingOverlayComponent(context);
    }

    protected UiTestContext Context { get; }
    protected OpenQA.Selenium.IWebDriver Driver => Context.Driver;
    protected Helpers.WaitHelper Wait => Context.Wait;
    protected Helpers.ElementHelper Element => Context.Element;

    public virtual void WaitUntilReady()
    {
        Wait.WaitForDocumentReady();
        _loadingOverlay.WaitUntilHidden();
    }

    protected void NavigateTo(string relativeRoute)
    {
        Driver.Navigate().GoToUrl(Context.Routes.ToAbsolute(relativeRoute));
        WaitUntilReady();
    }
}
