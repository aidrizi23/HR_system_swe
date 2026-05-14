using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Holidays;

public class CreateHolidayDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public bool IsRecurring { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
