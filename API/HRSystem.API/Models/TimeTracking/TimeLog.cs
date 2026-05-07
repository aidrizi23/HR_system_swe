using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.TimeTracking;

public class TimeLog : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
