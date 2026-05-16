using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Salary;

public class CreateBonusDto
{
    [Required, Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    [Required]
    public DateTime BonusDate { get; set; }
    public bool IsRecurring { get; set; } = false;
    [MaxLength(500)]
    public string? Reason { get; set; }
}
