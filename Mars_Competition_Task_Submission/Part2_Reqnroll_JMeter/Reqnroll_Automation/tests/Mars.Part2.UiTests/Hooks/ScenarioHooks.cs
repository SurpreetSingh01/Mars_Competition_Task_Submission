using Mars.Part2.UiTests.Config;
using Mars.Part2.UiTests.Drivers;
using Mars.Part2.UiTests.Helpers;
using Mars.Part2.UiTests.Models;
using Reqnroll;

namespace Mars.Part2.UiTests.Hooks;

[Binding]
public sealed class ScenarioHooks
{
    private readonly ScenarioContext _scenarioContext;

    public ScenarioHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario(Order = 0)]
    public void BeforeScenario()
    {
        var settings = ConfigReader.Load();
        var routes = new RouteMap(settings);
        var driver = DriverFactory.Create(settings);
        var wait = new WaitHelper(driver, settings.Timeouts.DefaultWaitSeconds);
        var element = new ElementHelper(driver, wait);
        var screenshots = new ScreenshotHelper(driver, settings.Artifacts.ScreenshotDirectory);
        var debugState = new DebugStateHelper(driver, settings.Artifacts.LogDirectory);

        var uiContext = new UiTestContext
        {
            Driver = driver,
            Settings = settings,
            Routes = routes,
            TestData = new TestDataReader(),
            Wait = wait,
            Element = element,
            Screenshots = screenshots,
            DebugState = debugState
        };

        _scenarioContext.Set(uiContext);
    }

    [AfterScenario(Order = 100)]
    public void AfterScenario()
    {
        if (!_scenarioContext.TryGetValue(out UiTestContext? uiContext) || uiContext is null)
        {
            return;
        }

        try
        {
            if (_scenarioContext.TestError is not null)
            {
                uiContext.Screenshots.Save(_scenarioContext.ScenarioInfo.Title);
                uiContext.DebugState.Save(_scenarioContext.ScenarioInfo.Title);
            }
        }
        finally
        {
            uiContext.Driver.Quit();
        }
    }
}
