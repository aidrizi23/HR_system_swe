namespace HRSystem.API.Services.Common;

public interface IApprovalScopeService
{
    Task<bool> IsInScopeAsync(int approverEmployeeId, int ownerEmployeeId);
    Task<HashSet<int>> GetScopeEmployeeIdsAsync(int approverEmployeeId);
    Task EnsureInScopeAsync(int approverEmployeeId, int ownerEmployeeId);
}
