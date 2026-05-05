using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Common;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Models.Employee;

public class Employee : BaseEntity, ISoftDeletable, ISlugEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public DateTime HireDate { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public string? JobTitle { get; set; }
    public int? DepartmentId { get; set; }
    public int? TeamId { get; set; }
    public int? ManagerId { get; set; }
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public decimal StandardWorkHoursPerDay { get; set; } = 8;
    public int StandardWorkDaysPerWeek { get; set; } = 5;
    public DateTime? TerminationDate { get; set; }

    public string Slug { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DepartmentEntity? Department { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public User? User { get; set; }
}
