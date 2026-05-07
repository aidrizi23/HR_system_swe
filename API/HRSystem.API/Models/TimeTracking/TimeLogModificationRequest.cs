using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.TimeTracking;

public class TimeLogModificationRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int TimeLogId { get; set; }
    public TimeSpan RequestedStartTime { get; set; }
    public TimeSpan RequestedEndTime { get; set; }
    public string? Reason { get; set; }
    public ModificationRequestStatus Status { get; set; } = ModificationRequestStatus.Pending;
    public int? ApprovedById { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public TimeLog TimeLog { get; set; } = null!;
}
