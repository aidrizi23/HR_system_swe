using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Salary;

public class SalaryRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
    public int ChangedById { get; set; }
}
