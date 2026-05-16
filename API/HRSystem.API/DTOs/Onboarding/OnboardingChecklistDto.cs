namespace HRSystem.API.DTOs.Onboarding;

public class OnboardingChecklistDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public List<OnboardingChecklistItemDto> Items { get; set; } = new();
}
