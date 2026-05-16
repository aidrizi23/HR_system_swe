using HRSystem.API.Models.TaskManagement;

namespace HRSystem.API.DTOs.Tasks;

public class WorkTaskDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public int AssignedById { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public WorkTaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CommentCount { get; set; }
}
