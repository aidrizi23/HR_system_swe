namespace HRSystem.API.DTOs.Overtime;

public class OvertimeRecordDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? RecommendedById { get; set; }
    public string? RecommenderComments { get; set; }
    public DateTime? RecommendedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? DetectedFromTimeLogId { get; set; }
    public DateTime CreatedAt { get; set; }
}
