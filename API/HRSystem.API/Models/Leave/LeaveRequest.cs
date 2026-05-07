using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Leave;

public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public string? AttachmentUrl { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public int? RecommendedById { get; set; }
    public string? RecommenderComments { get; set; }
    public DateTime? RecommendedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public LeaveType LeaveType { get; set; } = null!;
}
