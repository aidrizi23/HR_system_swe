namespace HRSystem.API.Services.Pdf;

public class EmploymentLetterModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime HireDate { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public DateTime GeneratedAtUtc { get; set; }
    public string CompanyName { get; set; } = "HR System";
}
