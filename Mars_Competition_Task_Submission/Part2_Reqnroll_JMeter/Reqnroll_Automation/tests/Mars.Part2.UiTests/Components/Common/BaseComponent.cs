using Mars.Part2.UiTests.Models;

namespace Mars.Part2.UiTests.Components.Common;

public abstract class BaseComponent
{
    protected BaseComponent(UiTestContext context)
    {
        Context = context;
    }

    protected UiTestContext Context { get; }
    protected OpenQA.Selenium.IWebDriver Driver => Context.Driver;
    protected Helpers.WaitHelper Wait => Context.Wait;
    protected Helpers.ElementHelper Element => Context.Element;
}
