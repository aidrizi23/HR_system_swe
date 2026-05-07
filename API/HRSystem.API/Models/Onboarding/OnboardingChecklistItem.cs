using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Onboarding;

public class OnboardingChecklistItem : BaseEntity
{
    public int ChecklistId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ResponsiblePartyId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public OnboardingItemStatus Status { get; set; } = OnboardingItemStatus.Pending;

    public OnboardingChecklist Checklist { get; set; } = null!;
    public EmployeeEntity? ResponsibleParty { get; set; }
}
