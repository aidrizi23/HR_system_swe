using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.TimeTracking;

public class CreateModificationRequestDto
{
    [Range(1, int.MaxValue)]
    public int TimeLogId { get; set; }

    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string RequestedStartTime { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string RequestedEndTime { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }
}
