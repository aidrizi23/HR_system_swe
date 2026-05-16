using HRSystem.API.DTOs.Salary;

namespace HRSystem.API.Services.Salary;

public interface ISalaryProjectionService
{
    Task<SalaryProjectionDto?> ComputeAsync(int employeeId, DateOnly? forDate = null);
}
