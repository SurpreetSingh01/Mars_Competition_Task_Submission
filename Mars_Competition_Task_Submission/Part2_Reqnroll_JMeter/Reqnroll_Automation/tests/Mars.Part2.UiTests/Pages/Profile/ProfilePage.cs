using Mars.Part2.UiTests.Components.Profile;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Auth;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Pages.Profile;

public sealed class ProfilePage : BasePage
{
    private const int LoginPromptGraceSeconds = 3;

    private static readonly By[] ProfileShellMarkers =
    {
        By.CssSelector("a[data-tab='third']"),
        By.CssSelector("a[data-tab='fourth']"),
        By.XPath("//a[contains(normalize-space(.),'Education')]"),
        By.XPath("//a[contains(normalize-space(.),'Certifications')]")
    };

    public ProfilePage(UiTestContext context) : base(context)
    {
        Education = new EducationComponent(context);
        Certifications = new CertificationsComponent(context);
    }

    public EducationComponent Education { get; }
    public CertificationsComponent Certifications { get; }

    public void Open()
    {
        NavigateTo(Context.Routes.Profile);
    }

    public void EnsureAuthenticated(Credentials credentials)
    {
        var loginPage = new LoginPage(Context);

        WaitUntilReady();

        if (loginPage.IsVisible() || loginPage.AppearsWithin(LoginPromptGraceSeconds))
        {
            loginPage.Login(credentials);
            WaitUntilReady();
        }

        if (loginPage.AppearsWithin(LoginPromptGraceSeconds))
        {
            loginPage.Login(credentials);
            WaitUntilReady();
        }

        if (loginPage.IsVisible())
        {
            throw new InvalidOperationException(
                $"Profile page is still showing a login prompt after authentication handling. {Context.DebugState.BuildPageStateSummary()}");
        }

        if (!HasProfileShell())
        {
            throw new InvalidOperationException(
                $"Profile page did not reach a stable ready state after authentication handling. {Context.DebugState.BuildPageStateSummary()}");
        }
    }

    public override void WaitUntilReady()
    {
        base.WaitUntilReady();

        var loginPage = new LoginPage(Context);
        Wait.TryWaitForCondition(_ => loginPage.IsVisible() || HasProfileShell(), 10);
    }

    private bool HasProfileShell()
    {
        return ProfileShellMarkers.Any(locator => Element.Exists(locator));
    }
}
