using HRSystem.API.Models.Performance;

namespace HRSystem.API.DTOs.Performance;

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int CycleId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string? SelfAssessmentText { get; set; }
    public int? SelfAssessmentRating { get; set; }
    public string? ManagerReviewText { get; set; }
    public int? ManagerRating { get; set; }
    public string? HRNotes { get; set; }
    public int? OverallRating { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReviewGoalDto> Goals { get; set; } = new();
}
