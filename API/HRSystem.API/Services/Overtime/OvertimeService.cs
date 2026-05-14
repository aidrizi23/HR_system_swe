using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Overtime;
using HRSystem.API.Models.Overtime;
using HRSystem.API.Services.Common;

namespace HRSystem.API.Services.Overtime;

public class OvertimeService : IOvertimeService
{
    private readonly AppDbContext _context;
    private readonly IApprovalScopeService _scope;

    public OvertimeService(AppDbContext context, IApprovalScopeService scope)
    {
        _context = context;
        _scope = scope;
    }

    public async Task<OvertimeRecordDto> SubmitManualAsync(int employeeId, CreateOvertimeRequestDto dto)
    {
        var rec = new OvertimeRecord
        {
            EmployeeId = employeeId,
            Date = dto.Date.Date,
            OvertimeMinutes = dto.OvertimeMinutes,
            Type = OvertimeType.ManualRequest,
            Reason = dto.Reason,
            Status = OvertimeStatus.Pending,
        };
        _context.Set<OvertimeRecord>().Add(rec);
        await _context.SaveChangesAsync();
        return await MapAsync(rec);
    }

    public async Task AutoDetectAsync(int employeeId, DateTime date, int totalMinutesToday, int sourceTimeLogId)
    {
        var employee = await _context.Employees.FirstAsync(e => e.Id == employeeId);
        var standardMinutes = (int)(employee.StandardWorkHoursPerDay * 60);
        if (totalMinutesToday <= standardMinutes) return;

        var overtimeMinutes = totalMinutesToday - standardMinutes;

        // Idempotency: if an AutoDetected record already exists for this date in a non-terminal
        // state, update it; otherwise create a new one.
        var existing = await _context.Set<OvertimeRecord>()
            .FirstOrDefaultAsync(r => r.EmployeeId == employeeId
                                   && r.Date == date.Date
                                   && r.Type == OvertimeType.AutoDetected
                                   && (r.Status == OvertimeStatus.Pending
                                    || r.Status == OvertimeStatus.Recommended));
        if (existing != null)
        {
            existing.OvertimeMinutes = overtimeMinutes;
            existing.DetectedFromTimeLogId = sourceTimeLogId;
        }
        else
        {
            _context.Set<OvertimeRecord>().Add(new OvertimeRecord
            {
                EmployeeId = employeeId,
                Date = date.Date,
                OvertimeMinutes = overtimeMinutes,
                Type = OvertimeType.AutoDetected,
                Status = OvertimeStatus.Pending,
                DetectedFromTimeLogId = sourceTimeLogId,
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<OvertimeRecordDto>> GetMineAsync(int employeeId)
    {
        var rows = await _context.Set<OvertimeRecord>()
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
        return await MapListAsync(rows);
    }

    public async Task<List<OvertimeRecordDto>> GetPendingInScopeAsync(int approverEmployeeId)
    {
        var scope = await _scope.GetScopeEmployeeIdsAsync(approverEmployeeId);

        var rows = await _context.Set<OvertimeRecord>()
            .Where(r => scope.Contains(r.EmployeeId)
                     && (r.Status == OvertimeStatus.Pending
                      || r.Status == OvertimeStatus.Recommended))
            .OrderBy(r => r.Date)
            .ToListAsync();
        return await MapListAsync(rows);
    }

    public async Task<List<OvertimeRecordDto>> ListAsync(OvertimeFilterDto filter)
    {
        var q = _context.Set<OvertimeRecord>().AsQueryable();
        if (filter.EmployeeId.HasValue) q = q.Where(r => r.EmployeeId == filter.EmployeeId.Value);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<OvertimeStatus>(filter.Status, out var s))
            q = q.Where(r => r.Status == s);
        if (filter.FromDate.HasValue) q = q.Where(r => r.Date >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) q = q.Where(r => r.Date <= filter.ToDate.Value.Date);
        var rows = await q.OrderByDescending(r => r.Date).ToListAsync();
        return await MapListAsync(rows);
    }

    public async Task<OvertimeRecordDto?> RecommendAsync(int recordId, int recommenderEmployeeId, string? comments)
    {
        var r = await _context.Set<OvertimeRecord>().FirstOrDefaultAsync(x => x.Id == recordId);
        if (r == null) return null;
        if (r.Status != OvertimeStatus.Pending)
            throw new InvalidOperationException($"Cannot recommend {r.Status} record");

        await _scope.EnsureInScopeAsync(recommenderEmployeeId, r.EmployeeId);

        r.Status = OvertimeStatus.Recommended;
        r.RecommendedById = recommenderEmployeeId;
        r.RecommenderComments = comments;
        r.RecommendedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await MapAsync(r);
    }

    public async Task<OvertimeRecordDto?> ApproveAsync(int recordId, int approverEmployeeId, string? comments)
    {
        var r = await _context.Set<OvertimeRecord>().FirstOrDefaultAsync(x => x.Id == recordId);
        if (r == null) return null;
        if (r.Status != OvertimeStatus.Recommended)
            throw new InvalidOperationException($"Cannot approve {r.Status} record; must be Recommended first");

        r.Status = OvertimeStatus.Approved;
        r.ApprovedById = approverEmployeeId;
        r.ApproverComments = comments;
        r.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await MapAsync(r);
    }

    public async Task<OvertimeRecordDto?> RejectAsync(int recordId, int actorEmployeeId, string reason, bool isHrActor)
    {
        var r = await _context.Set<OvertimeRecord>().FirstOrDefaultAsync(x => x.Id == recordId);
        if (r == null) return null;
        if (r.Status != OvertimeStatus.Pending && r.Status != OvertimeStatus.Recommended)
            throw new InvalidOperationException($"Cannot reject {r.Status} record");

        // Non-HR actors must be in the record owner's approval scope. HR/SuperAdmin act with global scope.
        if (!isHrActor)
            await _scope.EnsureInScopeAsync(actorEmployeeId, r.EmployeeId);

        r.Status = OvertimeStatus.Rejected;
        r.ApprovedById = actorEmployeeId;
        r.ApproverComments = reason;
        r.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await MapAsync(r);
    }

    private async Task<OvertimeRecordDto> MapAsync(OvertimeRecord r)
    {
        var name = await _context.Employees
            .Where(e => e.Id == r.EmployeeId)
            .Select(e => e.FirstName + " " + e.LastName)
            .FirstOrDefaultAsync();
        return new OvertimeRecordDto
        {
            Id = r.Id,
            PublicId = r.PublicId,
            EmployeeId = r.EmployeeId,
            EmployeeName = name,
            Date = r.Date,
            OvertimeMinutes = r.OvertimeMinutes,
            OvertimeHours = Math.Round((decimal)r.OvertimeMinutes / 60m, 2),
            Type = r.Type.ToString(),
            Reason = r.Reason,
            Status = r.Status.ToString(),
            RecommendedById = r.RecommendedById,
            RecommenderComments = r.RecommenderComments,
            RecommendedAt = r.RecommendedAt,
            ApprovedById = r.ApprovedById,
            ApproverComments = r.ApproverComments,
            ProcessedAt = r.ProcessedAt,
            DetectedFromTimeLogId = r.DetectedFromTimeLogId,
            CreatedAt = r.CreatedAt,
        };
    }

    private async Task<List<OvertimeRecordDto>> MapListAsync(List<OvertimeRecord> rows)
    {
        var names = await _context.Employees.ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);
        return rows.Select(r => new OvertimeRecordDto
        {
            Id = r.Id,
            PublicId = r.PublicId,
            EmployeeId = r.EmployeeId,
            EmployeeName = names.GetValueOrDefault(r.EmployeeId),
            Date = r.Date,
            OvertimeMinutes = r.OvertimeMinutes,
            OvertimeHours = Math.Round((decimal)r.OvertimeMinutes / 60m, 2),
            Type = r.Type.ToString(),
            Reason = r.Reason,
            Status = r.Status.ToString(),
            RecommendedById = r.RecommendedById,
            RecommenderComments = r.RecommenderComments,
            RecommendedAt = r.RecommendedAt,
            ApprovedById = r.ApprovedById,
            ApproverComments = r.ApproverComments,
            ProcessedAt = r.ProcessedAt,
            DetectedFromTimeLogId = r.DetectedFromTimeLogId,
            CreatedAt = r.CreatedAt,
        }).ToList();
    }
}
