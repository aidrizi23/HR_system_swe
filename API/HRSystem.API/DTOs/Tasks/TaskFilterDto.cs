using HRSystem.API.Models.TaskManagement;

namespace HRSystem.API.DTOs.Tasks;

public class TaskFilterDto
{
    public int? AssignedToId { get; set; }
    public WorkTaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
