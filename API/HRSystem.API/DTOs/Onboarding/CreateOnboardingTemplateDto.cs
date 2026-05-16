using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Onboarding;

public class CreateOnboardingTemplateDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? TargetEmploymentType { get; set; }

    public List<CreateOnboardingTemplateItemDto> Items { get; set; } = new();
}
