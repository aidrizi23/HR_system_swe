using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Leave;

public class LeaveType : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDaysPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
    public bool AllowCarryover { get; set; } = false;
    public int MaxCarryoverDays { get; set; } = 0;
    public bool RequiresAttachment { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public string Slug { get; set; } = string.Empty;
}
