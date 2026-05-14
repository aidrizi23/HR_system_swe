using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Leave;
using HRSystem.API.Services.Common;
using LeaveEntity = HRSystem.API.Models.Leave.LeaveRequest;
using LeaveTypeEntity = HRSystem.API.Models.Leave.LeaveType;
using LeaveBalanceEntity = HRSystem.API.Models.Leave.LeaveBalance;
using HRSystem.API.Models.Leave;

namespace HRSystem.API.Services.Leave;

public class LeaveService : ILeaveService
{
    private readonly AppDbContext _context;
    private readonly IApprovalScopeService _scope;

    public LeaveService(AppDbContext context, IApprovalScopeService scope)
    {
        _context = context;
        _scope = scope;
    }

    // ===================== Leave Types =====================

    public async Task<List<LeaveTypeDto>> GetTypesAsync()
    {
        return await _context.LeaveTypes
            .OrderBy(t => t.Name)
            .Select(t => MapType(t))
            .ToListAsync();
    }

    public async Task<LeaveTypeDto?> GetTypeByIdAsync(int id)
    {
        var t = await _context.LeaveTypes.FindAsync(id);
        return t == null ? null : MapType(t);
    }

    public async Task<LeaveTypeDto> CreateTypeAsync(CreateLeaveTypeDto dto)
    {
        var t = new LeaveTypeEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDaysPerYear = dto.DefaultDaysPerYear,
            IsPaid = dto.IsPaid,
            AllowCarryover = dto.AllowCarryover,
            MaxCarryoverDays = dto.MaxCarryoverDays,
            RequiresAttachment = dto.RequiresAttachment,
            IsActive = dto.IsActive,
            Slug = Slugify(dto.Name),
        };
        _context.LeaveTypes.Add(t);
        await _context.SaveChangesAsync();
        return MapType(t);
    }

    public async Task<LeaveTypeDto?> UpdateTypeAsync(int id, CreateLeaveTypeDto dto)
    {
        var t = await _context.LeaveTypes.FindAsync(id);
        if (t == null) return null;
        t.Name = dto.Name;
        t.Description = dto.Description;
        t.DefaultDaysPerYear = dto.DefaultDaysPerYear;
        t.IsPaid = dto.IsPaid;
        t.AllowCarryover = dto.AllowCarryover;
        t.MaxCarryoverDays = dto.MaxCarryoverDays;
        t.RequiresAttachment = dto.RequiresAttachment;
        t.IsActive = dto.IsActive;
        t.Slug = Slugify(dto.Name);
        await _context.SaveChangesAsync();
        return MapType(t);
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        var t = await _context.LeaveTypes.FindAsync(id);
        if (t == null) return false;
        // Refuse delete if balances or requests reference it
        var inUse = await _context.LeaveBalances.AnyAsync(b => b.LeaveTypeId == id)
                 || await _context.LeaveRequests.AnyAsync(r => r.LeaveTypeId == id);
        if (inUse)
            throw new InvalidOperationException("Leave type is in use by balances or requests");
        _context.LeaveTypes.Remove(t);
        await _context.SaveChangesAsync();
        return true;
    }

    // ===================== Balances =====================

    public async Task<List<LeaveBalanceDto>> GetMyBalancesAsync(int employeeId, int year)
    {
        return await _context.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .Include(b => b.LeaveType)
            .Select(b => MapBalance(b))
            .ToListAsync();
    }

    public async Task<List<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int year)
        => await GetMyBalancesAsync(employeeId, year);

    public async Task InitializeBalancesAsync(int year)
    {
        var employees = await _context.Employees
            .Where(e => e.Status == Models.Employee.EmploymentStatus.Active && !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync();

        var types = await _context.LeaveTypes
            .Where(t => t.IsActive)
            .ToListAsync();

        // existing balances to avoid duplicates
        var existingKeys = await _context.LeaveBalances
            .Where(b => b.Year == year)
            .Select(b => new { b.EmployeeId, b.LeaveTypeId })
            .ToListAsync();
        var existing = existingKeys
            .Select(k => (k.EmployeeId, k.LeaveTypeId))
            .ToHashSet();

        var toAdd = new List<LeaveBalanceEntity>();
        foreach (var employeeId in employees)
        {
            foreach (var type in types)
            {
                if (existing.Contains((employeeId, type.Id))) continue;
                toAdd.Add(new LeaveBalanceEntity
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = type.Id,
                    Year = year,
                    TotalDays = type.DefaultDaysPerYear,
                    UsedDays = 0,
                    CarriedOverDays = 0,
                });
            }
        }

        if (toAdd.Count > 0)
        {
            _context.LeaveBalances.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }
    }

    // ===================== Requests: Commit 1 =====================

    public async Task<LeaveRequestDto> SubmitAsync(int currentEmployeeId, CreateLeaveRequestDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new InvalidOperationException("EndDate must be on or after StartDate");

        var type = await _context.LeaveTypes.FindAsync(dto.LeaveTypeId);
        if (type == null || !type.IsActive)
            throw new InvalidOperationException("Leave type not available");

        var days = CountWeekdays(dto.StartDate, dto.EndDate);

        // Balance check (current year)
        var year = dto.StartDate.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == currentEmployeeId
                                   && b.LeaveTypeId == dto.LeaveTypeId
                                   && b.Year == year);
        if (balance == null)
            throw new InvalidOperationException($"No balance initialized for {year} / {type.Name}");

        var remaining = balance.TotalDays + balance.CarriedOverDays - balance.UsedDays;
        if (days > remaining)
            throw new InvalidOperationException($"Requested {days} days exceeds remaining {remaining}");

        if (type.RequiresAttachment && string.IsNullOrEmpty(dto.AttachmentUrl))
            throw new InvalidOperationException("This leave type requires an attachment");

        var req = new LeaveEntity
        {
            EmployeeId = currentEmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = days,
            Reason = dto.Reason,
            AttachmentUrl = dto.AttachmentUrl,
            Status = LeaveRequestStatus.Pending,
        };
        _context.LeaveRequests.Add(req);
        await _context.SaveChangesAsync();

        return await MapRequestAsync(req);
    }

    public async Task<List<LeaveRequestDto>> GetMineAsync(int currentEmployeeId)
    {
        var rows = await _context.LeaveRequests
            .Where(r => r.EmployeeId == currentEmployeeId)
            .Include(r => r.LeaveType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return await MapRequestsAsync(rows);
    }

    // ===================== Requests: Commit 2 — Two-tier approval =====================

    public async Task<List<LeaveRequestDto>> GetPendingInScopeAsync(int approverEmployeeId)
    {
        var scopeIds = await _scope.GetScopeEmployeeIdsAsync(approverEmployeeId);

        var rows = await _context.LeaveRequests
            .Where(r => scopeIds.Contains(r.EmployeeId)
                     && (r.Status == LeaveRequestStatus.Pending || r.Status == LeaveRequestStatus.Recommended))
            .Include(r => r.LeaveType)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
        return await MapRequestsAsync(rows);
    }

    public async Task<LeaveRequestDto?> RecommendAsync(int requestId, int recommenderEmployeeId, string? comments)
    {
        var req = await _context.LeaveRequests
            .Include(r => r.LeaveType)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (req == null) return null;
        if (req.Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException($"Can only recommend Pending requests (currently {req.Status})");

        await _scope.EnsureInScopeAsync(recommenderEmployeeId, req.EmployeeId);

        req.Status = LeaveRequestStatus.Recommended;
        req.RecommendedById = recommenderEmployeeId;
        req.RecommenderComments = comments;
        req.RecommendedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await MapRequestAsync(req);
    }

    public async Task<LeaveRequestDto?> ApproveAsync(int requestId, int approverEmployeeId, string? comments)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var req = await _context.LeaveRequests
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (req == null) return null;
            if (req.Status != LeaveRequestStatus.Recommended)
                throw new InvalidOperationException($"Can only approve Recommended requests (currently {req.Status})");

            // Decrement balance
            var year = req.StartDate.Year;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == req.EmployeeId
                                       && b.LeaveTypeId == req.LeaveTypeId
                                       && b.Year == year);
            if (balance == null)
                throw new InvalidOperationException("Balance not initialized for requested year");

            var remaining = balance.TotalDays + balance.CarriedOverDays - balance.UsedDays;
            if (req.TotalDays > remaining)
                throw new InvalidOperationException(
                    "The employee no longer has sufficient leave balance for approval");

            balance.UsedDays += req.TotalDays;

            req.Status = LeaveRequestStatus.Approved;
            req.ApprovedById = approverEmployeeId;
            req.ApproverComments = comments;
            req.ProcessedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return await MapRequestAsync(req);
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxAttempts)
            {
                // Another approval committed first; clear tracker and re-read on next attempt.
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Detached;
            }
        }
        throw new DbUpdateConcurrencyException(
            "Could not approve leave after 3 attempts due to concurrent balance updates");
    }

    public async Task<LeaveRequestDto?> RejectAsync(int requestId, int actorEmployeeId, string reason, bool isHrActor)
    {
        var req = await _context.LeaveRequests
            .Include(r => r.LeaveType)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (req == null) return null;
        if (req.Status == LeaveRequestStatus.Approved || req.Status == LeaveRequestStatus.Rejected
                || req.Status == LeaveRequestStatus.Cancelled)
            throw new InvalidOperationException($"Cannot reject a {req.Status} request");

        // Non-HR actors (TeamLead, DepartmentManager) must be in the request owner's approval scope
        // for both Pending and Recommended states. HR/SuperAdmin act with global scope.
        if (!isHrActor)
            await _scope.EnsureInScopeAsync(actorEmployeeId, req.EmployeeId);

        req.Status = LeaveRequestStatus.Rejected;
        req.ApproverComments = reason;
        req.ApprovedById = actorEmployeeId;
        req.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await MapRequestAsync(req);
    }

    public async Task<LeaveRequestDto?> CancelAsync(int requestId, int actorEmployeeId, bool isHrActor)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var req = await _context.LeaveRequests
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (req == null) return null;

            var isOwner = req.EmployeeId == actorEmployeeId;

            // Owner can self-cancel only while the request is still in Pending or Recommended state.
            // Once Approved, only HR/SuperAdmin may cancel (and refund the balance).
            if (isOwner
                    && (req.Status == LeaveRequestStatus.Pending || req.Status == LeaveRequestStatus.Recommended))
            {
                req.Status = LeaveRequestStatus.Cancelled;
                req.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return await MapRequestAsync(req);
            }

            if (isHrActor
                    && (req.Status == LeaveRequestStatus.Pending
                     || req.Status == LeaveRequestStatus.Recommended
                     || req.Status == LeaveRequestStatus.Approved))
            {
                if (req.Status == LeaveRequestStatus.Approved)
                {
                    var year = req.StartDate.Year;
                    var balance = await _context.LeaveBalances
                        .FirstOrDefaultAsync(b => b.EmployeeId == req.EmployeeId
                                               && b.LeaveTypeId == req.LeaveTypeId
                                               && b.Year == year);
                    if (balance != null) balance.UsedDays -= req.TotalDays;
                }

                req.Status = LeaveRequestStatus.Cancelled;
                req.ProcessedAt = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    return await MapRequestAsync(req);
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxAttempts)
                {
                    foreach (var entry in _context.ChangeTracker.Entries().ToList())
                        entry.State = EntityState.Detached;
                    continue;
                }
            }

            if (req.Status == LeaveRequestStatus.Rejected || req.Status == LeaveRequestStatus.Cancelled)
                throw new InvalidOperationException($"Cannot cancel a {req.Status} request");

            throw new UnauthorizedAccessException("Not authorized to cancel this request");
        }
        throw new DbUpdateConcurrencyException(
            "Could not cancel leave after 3 attempts due to concurrent balance updates");
    }

    // ===================== Helpers =====================

    private static decimal CountWeekdays(DateTime start, DateTime end)
    {
        var s = start.Date;
        var e = end.Date;
        int count = 0;
        for (var d = s; d <= e; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }
        return count;
    }

    private static string Slugify(string s)
    {
        var lower = s.ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        foreach (var c in lower)
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    private static LeaveTypeDto MapType(LeaveTypeEntity t) => new()
    {
        Id = t.Id,
        PublicId = t.PublicId,
        Name = t.Name,
        Description = t.Description,
        DefaultDaysPerYear = t.DefaultDaysPerYear,
        IsPaid = t.IsPaid,
        AllowCarryover = t.AllowCarryover,
        MaxCarryoverDays = t.MaxCarryoverDays,
        RequiresAttachment = t.RequiresAttachment,
        IsActive = t.IsActive,
        Slug = t.Slug,
        CreatedAt = t.CreatedAt,
    };

    private static LeaveBalanceDto MapBalance(LeaveBalanceEntity b) => new()
    {
        Id = b.Id,
        PublicId = b.PublicId,
        EmployeeId = b.EmployeeId,
        LeaveTypeId = b.LeaveTypeId,
        LeaveTypeName = b.LeaveType?.Name ?? "",
        Year = b.Year,
        TotalDays = b.TotalDays,
        UsedDays = b.UsedDays,
        CarriedOverDays = b.CarriedOverDays,
        RemainingDays = b.RemainingDays,
    };

    private async Task<LeaveRequestDto> MapRequestAsync(LeaveEntity r)
    {
        var employeeName = await _context.Employees
            .Where(e => e.Id == r.EmployeeId)
            .Select(e => e.FirstName + " " + e.LastName)
            .FirstOrDefaultAsync();
        var typeName = r.LeaveType?.Name ?? await _context.LeaveTypes
            .Where(t => t.Id == r.LeaveTypeId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync() ?? "";
        return MapRequest(r, employeeName, typeName);
    }

    private async Task<List<LeaveRequestDto>> MapRequestsAsync(IReadOnlyCollection<LeaveEntity> rows)
    {
        if (rows.Count == 0) return new List<LeaveRequestDto>();

        var employeeIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
        var names = await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);

        var missingTypeIds = rows.Where(r => r.LeaveType == null).Select(r => r.LeaveTypeId).Distinct().ToList();
        var fallbackTypes = missingTypeIds.Count == 0
            ? new Dictionary<int, string>()
            : await _context.LeaveTypes
                .Where(t => missingTypeIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Name);

        return rows.Select(r =>
        {
            var typeName = r.LeaveType?.Name ?? fallbackTypes.GetValueOrDefault(r.LeaveTypeId, "");
            return MapRequest(r, names.GetValueOrDefault(r.EmployeeId), typeName);
        }).ToList();
    }

    private static LeaveRequestDto MapRequest(LeaveEntity r, string? employeeName, string typeName) => new()
    {
        Id = r.Id,
        PublicId = r.PublicId,
        EmployeeId = r.EmployeeId,
        EmployeeName = employeeName,
        LeaveTypeId = r.LeaveTypeId,
        LeaveTypeName = typeName,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        TotalDays = r.TotalDays,
        Reason = r.Reason,
        AttachmentUrl = r.AttachmentUrl,
        Status = r.Status.ToString(),
        RecommendedById = r.RecommendedById,
        RecommenderComments = r.RecommenderComments,
        RecommendedAt = r.RecommendedAt,
        ApprovedById = r.ApprovedById,
        ApproverComments = r.ApproverComments,
        ProcessedAt = r.ProcessedAt,
        CreatedAt = r.CreatedAt,
    };
}
