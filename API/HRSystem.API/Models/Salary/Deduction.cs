using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Salary;

public class Deduction : BaseEntity
{
    public int EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsRecurring { get; set; }
    public DateTime EffectiveDate { get; set; }
}
