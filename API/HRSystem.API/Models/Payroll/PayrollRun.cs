using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Payroll;

public class PayrollRun : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    public DateTime? FinalizedAt { get; set; }
    public int? FinalizedById { get; set; }

    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
