using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Salary;

public class CreateSalaryRecordDto
{
    [Required, Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
    [Required]
    public DateTime EffectiveDate { get; set; }
    [MaxLength(500)]
    public string? Reason { get; set; }
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
}
