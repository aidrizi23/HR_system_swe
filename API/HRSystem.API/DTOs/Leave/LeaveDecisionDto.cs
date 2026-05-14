using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Leave;

public class LeaveDecisionDto
{
    [MaxLength(1000)]
    public string? Comments { get; set; }
}

public class LeaveRejectionDto
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
