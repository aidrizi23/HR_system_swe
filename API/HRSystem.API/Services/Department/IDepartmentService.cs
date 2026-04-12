using HRSystem.API.DTOs.Department;

namespace HRSystem.API.Services.Department;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task DeleteAsync(int id);
}
