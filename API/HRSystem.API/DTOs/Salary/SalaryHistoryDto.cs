namespace HRSystem.API.DTOs.Salary;

public class SalaryHistoryDto
{
    public int Id { get; set; }
    public decimal BaseSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
