namespace HRSystem.API.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}
