using Mars.Part2.UiTests.Models;

namespace Mars.Part2.UiTests.Helpers;

public static class RuntimeDataFactory
{
    public static ShareSkillData CreateUniqueShareSkill(ShareSkillData source)
    {
        var suffix = DateTime.Now.ToString("yyyyMMddHHmmss");

        return new ShareSkillData
        {
            Title = $"{source.Title} {suffix}",
            Description = source.Description,
            Category = source.Category,
            SubCategory = source.SubCategory,
            Tags = source.Tags,
            ServiceType = source.ServiceType,
            LocationType = source.LocationType,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            SkillTradeType = source.SkillTradeType,
            CreditAmount = source.CreditAmount,
            ActiveStatus = source.ActiveStatus
        };
    }
}
