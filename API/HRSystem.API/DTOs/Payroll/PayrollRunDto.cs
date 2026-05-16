using HRSystem.API.Models.Payroll;

namespace HRSystem.API.DTOs.Payroll;

public class PayrollRunDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollRunStatus Status { get; set; }
    public int PayslipCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
