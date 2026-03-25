using MarsAutomation.Helpers;
using MarsAutomation.Hooks;
using MarsAutomation.Pages;
using NUnit.Framework;

namespace MarsAutomation.Tests;

public class SearchTests : BaseTest
{
    [Test]
    public void SearchByKeyword_ReturnsResults()
    {
        var page = new SearchPage(Driver);
        page.NavigateToSearch();

        var keyword = JsonReader.GetString("search", "keyword");
        page.SearchByKeyword(keyword);

        Assert.That(page.HasResults(), Is.True, "Expected search results for keyword search.");
    }

    [Test]
    public void SearchByCategory_ReturnsResults()
    {
        var page = new SearchPage(Driver);
        page.NavigateToSearch();

        var category = JsonReader.GetString("search", "category");
        page.SearchByCategory(category);

        Assert.That(page.HasResults(), Is.True, "Expected search results for category search.");
    }
}

