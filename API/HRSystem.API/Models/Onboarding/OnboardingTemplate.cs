using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Onboarding;

public class OnboardingTemplate : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetEmploymentType { get; set; }

    public string Slug { get; set; } = string.Empty;

    public ICollection<OnboardingTemplateItem> Items { get; set; } = new List<OnboardingTemplateItem>();
}
