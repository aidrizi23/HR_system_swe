namespace HRSystem.API.DTOs.TimeTracking;

public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public List<TimeLogDto> Sessions { get; set; } = new();
    public int TotalMinutes { get; set; }
    public decimal TotalHours { get; set; }
    public int SessionCount { get; set; }
    public decimal StandardHours { get; set; }
    public bool IsOvertime { get; set; }
    public string? ActiveSessionStartTime { get; set; }
}
