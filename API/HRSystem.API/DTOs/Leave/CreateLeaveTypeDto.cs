using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Leave;

public class CreateLeaveTypeDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, 365)]
    public int DefaultDaysPerYear { get; set; }

    public bool IsPaid { get; set; } = true;
    public bool AllowCarryover { get; set; } = false;

    [Range(0, 365)]
    public int MaxCarryoverDays { get; set; } = 0;

    public bool RequiresAttachment { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
