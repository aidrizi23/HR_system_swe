using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Overtime;

public class OvertimeRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public int OvertimeMinutes { get; set; }
    public OvertimeType Type { get; set; }
    public string? Reason { get; set; }
    public OvertimeStatus Status { get; set; } = OvertimeStatus.Pending;
    public int? ApprovedById { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? DetectedFromTimeLogId { get; set; }
    public int? RecommendedById { get; set; }
    public string? RecommenderComments { get; set; }
    public DateTime? RecommendedAt { get; set; }
}
