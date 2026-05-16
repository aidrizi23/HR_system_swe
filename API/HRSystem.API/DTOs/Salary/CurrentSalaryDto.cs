namespace HRSystem.API.DTOs.Salary;

public class CurrentSalaryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public List<AllowanceDto> Allowances { get; set; } = new();
    public List<DeductionDto> Deductions { get; set; } = new();
}
