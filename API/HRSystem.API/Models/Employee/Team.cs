using HRSystem.API.Models.Common;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Models.Employee;

public class Team : BaseEntity, ISoftDeletable, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public int? TeamLeadId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DepartmentEntity? Department { get; set; }
    public Employee? TeamLead { get; set; }
    public ICollection<Employee> Members { get; set; } = new List<Employee>();
}
