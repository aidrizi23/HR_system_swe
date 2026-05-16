using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Payroll;

public class UpdatePayslipDto
{
    [Range(0, double.MaxValue)] public decimal? BaseSalary { get; set; }
    [Range(0, double.MaxValue)] public decimal? AllowancesTotal { get; set; }
    [Range(0, double.MaxValue)] public decimal? BonusesTotal { get; set; }
    [Range(0, double.MaxValue)] public decimal? DeductionsTotal { get; set; }
}
