using HRSystem.API.Models.Salary;

namespace HRSystem.API.DTOs.Salary;

public class AllowanceDto
{
    public int Id { get; set; }
    public AllowanceType Type { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
