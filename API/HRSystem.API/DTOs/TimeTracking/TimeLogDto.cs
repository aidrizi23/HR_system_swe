namespace HRSystem.API.DTOs.TimeTracking;

public class TimeLogDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty; // "HH:mm"
    public string? EndTime { get; set; }                   // "HH:mm" or null while open
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
