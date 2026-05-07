using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Performance;

public class PerformanceReview : BaseEntity
{
    public int CycleId { get; set; }
    public int EmployeeId { get; set; }
    public int ManagerId { get; set; }
    public string? SelfAssessmentText { get; set; }
    public int? SelfAssessmentRating { get; set; }
    public string? ManagerReviewText { get; set; }
    public int? ManagerRating { get; set; }
    public string? HRNotes { get; set; }
    public int? OverallRating { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.PendingSelfAssessment;

    public ReviewCycle Cycle { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
    public EmployeeEntity Manager { get; set; } = null!;
    public ICollection<ReviewGoal> Goals { get; set; } = new List<ReviewGoal>();
}
