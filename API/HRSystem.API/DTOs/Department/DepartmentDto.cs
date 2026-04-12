namespace HRSystem.API.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
