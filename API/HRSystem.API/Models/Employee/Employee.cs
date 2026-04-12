using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Employee;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public int? DepartmentId { get; set; }
}
