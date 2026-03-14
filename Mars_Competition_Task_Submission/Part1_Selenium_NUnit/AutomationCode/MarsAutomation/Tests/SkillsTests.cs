using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using NUnit.Framework;

namespace MarsAutomation.Tests;

public class SkillsTests : BaseTest
{
    [Test]
    public void CanAddUpdateAndDeleteSkill()
    {
        var profilePage = new ProfilePage(Driver);
        profilePage.Open();
        profilePage.WaitForProfilePageToLoad();

        var skillsPage = new SkillsPage(Driver);

        skillsPage.AddSkill("C#", "Beginner");
        skillsPage.UpdateSkillLevel("C#", "Expert");
        skillsPage.DeleteSkill("C#");
    }
}

