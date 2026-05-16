using System.ComponentModel.DataAnnotations;
using HRSystem.API.Models.TaskManagement;

namespace HRSystem.API.DTOs.Tasks;

public class CreateWorkTaskDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required]
    public int AssignedToId { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(200)]
    public string? Tags { get; set; }
}
