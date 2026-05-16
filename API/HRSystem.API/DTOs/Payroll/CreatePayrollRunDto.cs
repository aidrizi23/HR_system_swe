using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Payroll;

public class CreatePayrollRunDto
{
    [Required, Range(2000, 2100)]
    public int Year { get; set; }

    [Required, Range(1, 12)]
    public int Month { get; set; }
}
