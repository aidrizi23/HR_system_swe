using HRSystem.API.DTOs.Leave;

namespace HRSystem.API.Services.Leave;

public interface ILeaveService
{
    // ----- Leave Types -----
    Task<List<LeaveTypeDto>> GetTypesAsync();
    Task<LeaveTypeDto?> GetTypeByIdAsync(int id);
    Task<LeaveTypeDto> CreateTypeAsync(CreateLeaveTypeDto dto);
    Task<LeaveTypeDto?> UpdateTypeAsync(int id, CreateLeaveTypeDto dto);
    Task<bool> DeleteTypeAsync(int id);

    // ----- Balances -----
    Task<List<LeaveBalanceDto>> GetMyBalancesAsync(int employeeId, int year);
    Task<List<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int year);
    Task InitializeBalancesAsync(int year);

    // ----- Requests: Commit 1 -----
    Task<LeaveRequestDto> SubmitAsync(int currentEmployeeId, CreateLeaveRequestDto dto);
    Task<List<LeaveRequestDto>> GetMineAsync(int currentEmployeeId);

    // ----- Requests: Commit 2 (added in two-tier workflow commit) -----
    Task<List<LeaveRequestDto>> GetPendingInScopeAsync(int approverEmployeeId);
    Task<LeaveRequestDto?> RecommendAsync(int requestId, int recommenderEmployeeId, string? comments);
    Task<LeaveRequestDto?> ApproveAsync(int requestId, int approverEmployeeId, string? comments);
    Task<LeaveRequestDto?> RejectAsync(int requestId, int actorEmployeeId, string reason, bool isHrActor);
    Task<LeaveRequestDto?> CancelAsync(int requestId, int actorEmployeeId, bool isHrActor);
}
