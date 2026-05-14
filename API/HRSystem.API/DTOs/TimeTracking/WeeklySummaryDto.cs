namespace HRSystem.API.DTOs.TimeTracking;

public class WeeklySummaryDto
{
    public DateTime WeekStart { get; set; }
    public List<DailySummaryDto> Days { get; set; } = new();
    public int TotalMinutes { get; set; }
    public decimal TotalHours { get; set; }
    public decimal StandardWeeklyHours { get; set; }
}
