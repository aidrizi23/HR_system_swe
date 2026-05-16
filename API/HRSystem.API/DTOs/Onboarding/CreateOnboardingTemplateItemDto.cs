using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Onboarding;

public class CreateOnboardingTemplateItemDto
{
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string ResponsibleRole { get; set; } = "Employee";

    [Range(1, 365)]
    public int DefaultDueDays { get; set; } = 1;
}
