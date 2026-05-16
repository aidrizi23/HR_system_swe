using System.ComponentModel.DataAnnotations;
using HRSystem.API.Models.Salary;

namespace HRSystem.API.DTOs.Salary;

public class CreateAllowanceDto
{
    [Required]
    public AllowanceType Type { get; set; }
    [Required, Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    public bool IsRecurring { get; set; } = true;
    [MaxLength(500)]
    public string? Notes { get; set; }
}
