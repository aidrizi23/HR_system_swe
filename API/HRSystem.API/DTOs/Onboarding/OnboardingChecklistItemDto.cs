namespace HRSystem.API.DTOs.Onboarding;

public class OnboardingChecklistItemDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ResponsiblePartyId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;   // Pending | Completed | Overdue (computed)
}
