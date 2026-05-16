using HRSystem.API.DTOs.Salary;

namespace HRSystem.API.Services.Salary;

public interface ISalaryService
{
    Task<CurrentSalaryDto?> GetMineAsync(int currentEmployeeId);
    Task<CurrentSalaryDto?> GetByEmployeeAsync(int employeeId);
    Task<List<SalaryHistoryDto>> GetHistoryAsync(int employeeId);
    Task<CurrentSalaryDto> CreateRecordAsync(int employeeId, CreateSalaryRecordDto dto);
    Task<AllowanceDto> AddAllowanceAsync(int employeeId, CreateAllowanceDto dto);
    Task<BonusDto> AddBonusAsync(int employeeId, CreateBonusDto dto);
    Task<DeductionDto> AddDeductionAsync(int employeeId, CreateDeductionDto dto);
    Task<bool> RemoveAllowanceAsync(int id);
    Task<bool> RemoveBonusAsync(int id);
    Task<bool> RemoveDeductionAsync(int id);
}
