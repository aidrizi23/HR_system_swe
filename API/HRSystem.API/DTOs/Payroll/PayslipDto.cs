using HRSystem.API.Models.Payroll;

namespace HRSystem.API.DTOs.Payroll;

public class PayslipDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int PayrollRunId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal AllowancesTotal { get; set; }
    public decimal BonusesTotal { get; set; }
    public decimal DeductionsTotal { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public string Currency { get; set; } = "USD";
    public PayslipStatus Status { get; set; }
    public string? PdfFilePath { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
