using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;

namespace HRSystem.API.Services.Common;

public class ApprovalScopeService : IApprovalScopeService
{
    private readonly AppDbContext _context;

    public ApprovalScopeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsInScopeAsync(int approverEmployeeId, int ownerEmployeeId)
    {
        var owner = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == ownerEmployeeId);
        if (owner == null) return false;
        return (owner.Team != null && owner.Team.TeamLeadId == approverEmployeeId)
            || (owner.Department != null && owner.Department.HeadId == approverEmployeeId);
    }

    public async Task<HashSet<int>> GetScopeEmployeeIdsAsync(int approverEmployeeId)
    {
        var teamMemberIds = await _context.Employees
            .Where(e => e.Team != null && e.Team.TeamLeadId == approverEmployeeId)
            .Select(e => e.Id)
            .ToListAsync();
        var deptMemberIds = await _context.Employees
            .Where(e => e.Department != null && e.Department.HeadId == approverEmployeeId)
            .Select(e => e.Id)
            .ToListAsync();
        return teamMemberIds.Union(deptMemberIds).ToHashSet();
    }

    public async Task EnsureInScopeAsync(int approverEmployeeId, int ownerEmployeeId)
    {
        if (!await IsInScopeAsync(approverEmployeeId, ownerEmployeeId))
            throw new UnauthorizedAccessException("Not in approval scope for this request");
    }
}
