namespace HRSystem.API.DTOs.Leave;

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedOverDays { get; set; }
    public decimal RemainingDays { get; set; }
}
