using Mars.Part2.UiTests.Config;
using Mars.Part2.UiTests.Helpers;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Models;

public sealed class UiTestContext
{
    public required IWebDriver Driver { get; init; }
    public required TestSettings Settings { get; init; }
    public required RouteMap Routes { get; init; }
    public required TestDataReader TestData { get; init; }
    public required WaitHelper Wait { get; init; }
    public required ElementHelper Element { get; init; }
    public required ScreenshotHelper Screenshots { get; init; }
    public required DebugStateHelper DebugState { get; init; }
}
