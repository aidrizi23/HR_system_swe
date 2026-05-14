using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Leave;

public class CreateLeaveRequestDto
{
    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}
