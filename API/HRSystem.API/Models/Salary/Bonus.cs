using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Salary;

public class Bonus : BaseEntity
{
    public int EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
    public bool IsRecurring { get; set; }
}
