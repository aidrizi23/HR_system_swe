using HRSystem.API.DTOs.TimeTracking;

namespace HRSystem.API.Services.TimeTracking;

public interface ITimeTrackingService
{
    Task<TimeLogDto> ClockInAsync(int employeeId);
    Task<TimeLogDto?> ClockOutAsync(int employeeId);
    Task<TimeLogDto> CreateManualEntryAsync(int employeeId, CreateManualTimeLogDto dto);
    Task<List<TimeLogDto>> GetMineAsync(int employeeId, DateTime? date);
    Task<List<TimeLogDto>> GetTeamAsync(int approverEmployeeId, DateTime date);
    Task<DailySummaryDto> GetDailySummaryAsync(int employeeId, DateTime date);
    Task<WeeklySummaryDto> GetWeeklySummaryAsync(int employeeId, DateTime weekStart);
    Task<ModificationRequestDto> CreateModificationAsync(int employeeId, CreateModificationRequestDto dto);
    Task<List<ModificationRequestDto>> GetMyModificationsAsync(int employeeId);
    Task<List<ModificationRequestDto>> GetPendingModificationsInScopeAsync(int approverEmployeeId);
    Task<ModificationRequestDto?> ApproveModificationAsync(int requestId, int approverEmployeeId);
    Task<ModificationRequestDto?> RejectModificationAsync(int requestId, int approverEmployeeId, string reason);
}
