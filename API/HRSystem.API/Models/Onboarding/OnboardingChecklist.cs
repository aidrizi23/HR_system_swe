using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Onboarding;

public class OnboardingChecklist : BaseEntity
{
    public int EmployeeId { get; set; }
    public int TemplateId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public OnboardingChecklistStatus Status { get; set; } = OnboardingChecklistStatus.InProgress;

    public EmployeeEntity Employee { get; set; } = null!;
    public OnboardingTemplate Template { get; set; } = null!;
    public ICollection<OnboardingChecklistItem> Items { get; set; } = new List<OnboardingChecklistItem>();
}
