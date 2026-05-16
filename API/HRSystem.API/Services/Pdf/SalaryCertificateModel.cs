namespace HRSystem.API.Services.Pdf;

public class SalaryCertificateModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public decimal BaseSalary { get; set; }
    public List<(string Type, decimal Amount)> Allowances { get; set; } = new();
    public decimal TotalCompensation { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime GeneratedAtUtc { get; set; }
    public string CompanyName { get; set; } = "HR System";
}
