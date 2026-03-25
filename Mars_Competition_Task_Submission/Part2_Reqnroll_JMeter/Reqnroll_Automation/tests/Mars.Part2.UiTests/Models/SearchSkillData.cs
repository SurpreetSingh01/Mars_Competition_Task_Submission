namespace Mars.Part2.UiTests.Models;

public sealed class SearchSkillData
{
    public string SearchTerm { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string ExpectedResultText { get; set; } = string.Empty;
}
