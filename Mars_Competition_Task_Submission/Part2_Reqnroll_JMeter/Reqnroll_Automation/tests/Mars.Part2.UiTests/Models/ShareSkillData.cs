namespace Mars.Part2.UiTests.Models;

public sealed class ShareSkillData
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Hourly";
    public string LocationType { get; set; } = "Online";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string AvailableDayIndex { get; set; } = "1";
    public string StartTime { get; set; } = "09:00";
    public string EndTime { get; set; } = "17:00";
    public string SkillTradeType { get; set; } = "Credit";
    public string CreditAmount { get; set; } = string.Empty;
    public string ActiveStatus { get; set; } = "Active";
}
