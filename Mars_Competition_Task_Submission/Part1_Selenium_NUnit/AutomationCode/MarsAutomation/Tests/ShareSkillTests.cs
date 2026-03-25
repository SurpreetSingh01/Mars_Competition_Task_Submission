using MarsAutomation.Helpers;
using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace MarsAutomation.Tests;

public class ShareSkillTests : BaseTest
{
    [Test]
    public void CanCreateAndDeleteListing()
    {
        var page = new ShareSkillPage(Driver);
        page.NavigateToShareSkill();

        var title = "Listing " + DateTime.Now.Ticks;
        var description = "Automated listing";
        var category = JsonReader.GetString("search", "category");
        var tag = JsonReader.GetString("skills", "skillName");

        page.CreateListing(title, description, category, tag);
        page.DeleteListing(title);
    }

    [Test]
    public void ValidateRequiredFields()
    {
        var page = new ShareSkillPage(Driver);
        page.NavigateToShareSkill();

        // Intentionally click save with empty fields
        page.SubmitEmptyForm();

        var messages = page.GetValidationMessages();
        Assert.That(messages, Is.Not.Empty, "Expected validation messages when required fields are empty.");
    }
}

