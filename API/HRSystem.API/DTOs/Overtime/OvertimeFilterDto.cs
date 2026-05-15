namespace HRSystem.API.DTOs.Overtime;

public class OvertimeFilterDto
{
    public int? EmployeeId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
