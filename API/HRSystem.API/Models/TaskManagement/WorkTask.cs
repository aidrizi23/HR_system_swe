using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.TaskManagement;

public class WorkTask : BaseEntity, ISlugEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public int AssignedById { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.ToDo;
    public DateTime? DueDate { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string Slug { get; set; } = string.Empty;

    public EmployeeEntity AssignedTo { get; set; } = null!;
    public EmployeeEntity AssignedBy { get; set; } = null!;
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
