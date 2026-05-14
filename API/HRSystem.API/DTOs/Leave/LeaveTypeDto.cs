namespace HRSystem.API.DTOs.Leave;

public class LeaveTypeDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDaysPerYear { get; set; }
    public bool IsPaid { get; set; }
    public bool AllowCarryover { get; set; }
    public int MaxCarryoverDays { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool IsActive { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
