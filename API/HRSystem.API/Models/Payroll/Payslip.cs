using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Payroll;

public class Payslip : BaseEntity
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal AllowancesTotal { get; set; }
    public decimal BonusesTotal { get; set; }
    public decimal DeductionsTotal { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public string Currency { get; set; } = "USD";
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public DateTime? FinalizedAt { get; set; }
    public string? PdfFilePath { get; set; }

    public PayrollRun PayrollRun { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
}
