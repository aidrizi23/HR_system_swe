using HRSystem.API.Models.Performance;

namespace HRSystem.API.DTOs.Performance;

public class ReviewGoalDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int ReviewId { get; set; }
    public int EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; }
    public bool CarriedForward { get; set; }
    public DateTime CreatedAt { get; set; }
}
