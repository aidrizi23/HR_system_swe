using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Performance;

public class ReviewGoal : BaseEntity
{
    public int ReviewId { get; set; }
    public int EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public bool CarriedForward { get; set; }

    public PerformanceReview Review { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
}
