using HRSystem.API.Models.Common;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Models.Performance;

public class ReviewCycle : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReviewCycleStatus Status { get; set; } = ReviewCycleStatus.Draft;
    public ReviewTargetScope TargetScope { get; set; } = ReviewTargetScope.All;
    public int? TargetDepartmentId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public DepartmentEntity? TargetDepartment { get; set; }
    public ICollection<PerformanceReview> Reviews { get; set; } = new List<PerformanceReview>();
}
