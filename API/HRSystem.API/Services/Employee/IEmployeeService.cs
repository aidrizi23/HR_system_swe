using HRSystem.API.DTOs.Employee;

namespace HRSystem.API.Services.Employee;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task DeleteAsync(int id);
}
