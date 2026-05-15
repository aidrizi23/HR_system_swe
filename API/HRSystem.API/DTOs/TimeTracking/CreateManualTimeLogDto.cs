namespace HRSystem.API.DTOs.TimeTracking;

// Manual time entry — employee retroactively logs hours for a past or current date.
public class CreateManualTimeLogDto
{
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public string? Notes { get; set; }
}
