namespace HRSystem.API.Services.Pdf;

public class PayslipPdfModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Bonuses { get; set; }
    public decimal Deductions { get; set; }
    public decimal Gross { get; set; }
    public decimal Net { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string CompanyName { get; set; } = "HR System";
}
