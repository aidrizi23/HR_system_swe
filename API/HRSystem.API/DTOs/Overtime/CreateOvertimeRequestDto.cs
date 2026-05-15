using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Overtime;

public class CreateOvertimeRequestDto
{
    [Required]
    public DateTime Date { get; set; }

    [Range(1, 720)]
    public int OvertimeMinutes { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }
}
