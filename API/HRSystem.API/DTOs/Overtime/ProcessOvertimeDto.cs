using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Overtime;

public class ProcessOvertimeDto
{
    [MaxLength(1000)]
    public string? Comments { get; set; }
}

public class RejectOvertimeDto
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
