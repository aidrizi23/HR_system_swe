using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Onboarding;

public class OnboardingTemplateItem : BaseEntity
{
    public int TemplateId { get; set; }
    public string Description { get; set; } = string.Empty;
    public ResponsibleRole ResponsibleRole { get; set; } = ResponsibleRole.Employee;
    public int DefaultDueDays { get; set; }

    public OnboardingTemplate Template { get; set; } = null!;
}
