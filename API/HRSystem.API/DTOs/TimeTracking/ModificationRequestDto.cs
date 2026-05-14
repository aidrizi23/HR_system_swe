namespace HRSystem.API.DTOs.TimeTracking;

public class ModificationRequestDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int TimeLogId { get; set; }
    public string RequestedStartTime { get; set; } = string.Empty;
    public string RequestedEndTime { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
