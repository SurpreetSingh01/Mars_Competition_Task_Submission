using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using MarsAutomation.Helpers;
using NUnit.Framework;

namespace MarsAutomation.Tests;

public class LanguagesTests : BaseTest
{
    [Test]
    public void CanAddUpdateAndDeleteLanguage()
    {
        var profilePage = new ProfilePage(Driver);
        profilePage.Open();
        profilePage.WaitForProfilePageToLoad();

        var languages = profilePage.Languages;

        var language = JsonReader.GetString("languages", "language");
        var level = JsonReader.GetString("languages", "level");

        languages.AddLanguage(language, level);
        languages.UpdateLanguageLevel(language, level);
        languages.DeleteLanguage(language);
    }
}

