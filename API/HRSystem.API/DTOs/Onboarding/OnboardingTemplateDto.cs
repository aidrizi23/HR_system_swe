namespace HRSystem.API.DTOs.Onboarding;

public class OnboardingTemplateDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetEmploymentType { get; set; }
    public string Slug { get; set; } = string.Empty;
    public List<OnboardingTemplateItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
