using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.TimeTracking;

public class UpdateTimeLogDto
{
    [RegularExpression(@"^\d{2}:\d{2}$")]
    public string? StartTime { get; set; }

    [RegularExpression(@"^\d{2}:\d{2}$")]
    public string? EndTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
