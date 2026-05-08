using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Salary;

public class Allowance : BaseEntity
{
    public int EmployeeId { get; set; }
    public AllowanceType Type { get; set; } = AllowanceType.Custom;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsRecurring { get; set; }
    public DateTime EffectiveDate { get; set; }
}
