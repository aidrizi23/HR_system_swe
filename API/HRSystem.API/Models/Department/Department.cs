using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Department;

public class Department : BaseEntity, ISoftDeletable, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? HeadId { get; set; }
    public int? ParentDepartmentId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Department? ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
