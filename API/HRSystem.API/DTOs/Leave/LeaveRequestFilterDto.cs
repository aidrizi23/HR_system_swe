namespace HRSystem.API.DTOs.Leave;

public class LeaveRequestFilterDto
{
    public int? EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
