namespace HRSystem.API.DTOs.Onboarding;

public class OnboardingTemplateItemDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ResponsibleRole { get; set; } = string.Empty;
    public int DefaultDueDays { get; set; }
}
