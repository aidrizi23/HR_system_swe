using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.TaskManagement;

public class TaskComment : BaseEntity
{
    public int TaskId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;

    public WorkTask Task { get; set; } = null!;
    public EmployeeEntity Author { get; set; } = null!;
}
