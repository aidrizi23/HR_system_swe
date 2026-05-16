using HRSystem.API.Models.Performance;

namespace HRSystem.API.DTOs.Performance;

public class ReviewCycleDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReviewCycleStatus Status { get; set; }
    public ReviewTargetScope TargetScope { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
