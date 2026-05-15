using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.TimeTracking;

public class CreateTimeLogDto
{
    [Required]
    public DateTime Date { get; set; }

    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string StartTime { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string EndTime { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
