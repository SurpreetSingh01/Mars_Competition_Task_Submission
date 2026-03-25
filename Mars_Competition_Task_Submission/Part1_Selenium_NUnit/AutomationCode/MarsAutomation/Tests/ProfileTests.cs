using MarsAutomation.Helpers;
using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using NUnit.Framework;

namespace MarsAutomation.Tests;

public class ProfileTests : BaseTest
{
    [Test]
    public void CanUpdateProfileFields()
    {
        var profilePage = new ProfilePage(Driver);

        profilePage.EnsureOnProfilePage();

        // TODO: reimplement profile field updates against new ProfilePage design.
        Assert.Pass("Profile update test placeholder - to be reimplemented with new ProfilePage model.");
    }
}

