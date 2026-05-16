using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Salary;

public class CreateDeductionDto
{
    [Required, MaxLength(300)]
    public string Description { get; set; } = string.Empty;
    [Required, Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    public bool IsRecurring { get; set; } = true;
}
