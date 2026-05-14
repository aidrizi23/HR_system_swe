using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.TimeTracking;

public class ProcessModificationDto
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
