using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Department;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
}
