using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.TimeTracking;
using HRSystem.API.Models.TimeTracking;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Notifications;
using HRSystem.API.Services.Overtime;
using NotifType = HRSystem.API.Models.Notifications.NotificationType;

namespace HRSystem.API.Services.TimeTracking;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly AppDbContext _context;
    private readonly IOvertimeService _overtime;
    private readonly IApprovalScopeService _scope;
    private readonly INotificationService _notification;

    public TimeTrackingService(AppDbContext context, IOvertimeService overtime, IApprovalScopeService scope, INotificationService notification)
    {
        _context = context;
        _overtime = overtime;
        _scope = scope;
        _notification = notification;
    }

    // ============== Clock-in / clock-out ==============

    public async Task<TimeLogDto> ClockInAsync(int employeeId)
    {
        var openSession = await _context.TimeLogs
            .FirstOrDefaultAsync(l => l.EmployeeId == employeeId && l.EndTime == null);
        if (openSession != null)
            throw new InvalidOperationException("There is already an open session — clock out first");

        var now = DateTime.UtcNow;
        var log = new TimeLog
        {
            EmployeeId = employeeId,
            Date = now.Date,
            StartTime = new TimeSpan(now.Hour, now.Minute, 0),
            EndTime = null,
            DurationMinutes = 0,
        };
        _context.TimeLogs.Add(log);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A concurrent ClockIn won the partial unique index race
            // (IX_TimeLogs_OpenSession_EmployeeId).
            throw new InvalidOperationException("There is already an open session — clock out first");
        }
        return Map(log);
    }

    public async Task<TimeLogDto?> ClockOutAsync(int employeeId)
    {
        var open = await _context.TimeLogs
            .FirstOrDefaultAsync(l => l.EmployeeId == employeeId && l.EndTime == null);
        if (open == null) return null;

        var now = DateTime.UtcNow;
        var endTime = new TimeSpan(now.Hour, now.Minute, 0);
        var elapsed = endTime - open.StartTime;
        if (elapsed.Ticks < 0)
            elapsed = elapsed.Add(TimeSpan.FromDays(1));
        if (elapsed.TotalHours > 16)
            throw new InvalidOperationException(
                "Session exceeds 16 hours — please log this entry manually instead of clocking out");

        open.EndTime = endTime;
        open.DurationMinutes = (int)elapsed.TotalMinutes;
        await _context.SaveChangesAsync();

        // Compute today's total minutes for this employee and auto-detect overtime
        var todayTotal = await _context.TimeLogs
            .Where(l => l.EmployeeId == employeeId && l.Date == open.Date)
            .SumAsync(l => l.DurationMinutes);
        await _overtime.AutoDetectAsync(employeeId, open.Date, todayTotal, open.Id);

        return Map(open);
    }

    public async Task<TimeLogDto> CreateManualEntryAsync(int employeeId, CreateManualTimeLogDto dto)
    {
        if (dto.Hours <= 0 || dto.Hours > 16)
            throw new InvalidOperationException("Hours must be between 0 and 16");
        if (AsUtcDate(dto.Date) > AsUtcDate(DateTime.UtcNow))
            throw new InvalidOperationException("Cannot log time for a future date");

        var minutes = (int)Math.Round(dto.Hours * 60m);
        var start = new TimeSpan(9, 0, 0); // 09:00 anchor for retroactive entries
        var end = start.Add(TimeSpan.FromMinutes(minutes));

        var log = new TimeLog
        {
            EmployeeId = employeeId,
            Date = AsUtcDate(dto.Date),
            StartTime = start,
            EndTime = end,
            DurationMinutes = minutes,
            Notes = dto.Notes,
        };
        _context.TimeLogs.Add(log);
        await _context.SaveChangesAsync();

        // Day total may now exceed standard work hours — auto-detect overtime same as ClockOut.
        var dayTotal = await _context.TimeLogs
            .Where(l => l.EmployeeId == employeeId && l.Date == log.Date)
            .SumAsync(l => l.DurationMinutes);
        await _overtime.AutoDetectAsync(employeeId, log.Date, dayTotal, log.Id);

        return Map(log);
    }

    // ============== Lists ==============

    public async Task<List<TimeLogDto>> GetMineAsync(int employeeId, DateTime? date)
    {
        var q = _context.TimeLogs.Where(l => l.EmployeeId == employeeId);
        if (date.HasValue)
        {
            var d = AsUtcDate(date.Value);
            q = q.Where(l => l.Date == d);
        }
        var rows = await q.OrderByDescending(l => l.Date).ThenBy(l => l.StartTime).ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<List<TimeLogDto>> GetTeamAsync(int approverEmployeeId, DateTime date)
    {
        var scope = await _scope.GetScopeEmployeeIdsAsync(approverEmployeeId);
        var d = AsUtcDate(date);
        var rows = await _context.TimeLogs
            .Where(l => scope.Contains(l.EmployeeId) && l.Date == d)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
        return rows.Select(Map).ToList();
    }

    // ============== Summaries ==============

    public async Task<DailySummaryDto> GetDailySummaryAsync(int employeeId, DateTime date)
    {
        var d = AsUtcDate(date);
        var sessions = await _context.TimeLogs
            .Where(l => l.EmployeeId == employeeId && l.Date == d)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
        var totalMinutes = sessions.Sum(s => s.DurationMinutes);
        var standardHours = (await _context.Employees.Where(e => e.Id == employeeId)
            .Select(e => e.StandardWorkHoursPerDay).FirstOrDefaultAsync());
        var employeeName = await _context.Employees.Where(e => e.Id == employeeId)
            .Select(e => e.FirstName + " " + e.LastName).FirstOrDefaultAsync();
        var active = sessions.FirstOrDefault(s => s.EndTime == null);

        return new DailySummaryDto
        {
            Date = date.Date,
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            Sessions = sessions.Select(Map).ToList(),
            TotalMinutes = totalMinutes,
            TotalHours = Math.Round((decimal)totalMinutes / 60m, 2),
            SessionCount = sessions.Count,
            StandardHours = standardHours,
            IsOvertime = (decimal)totalMinutes / 60m > standardHours,
            ActiveSessionStartTime = active != null ? FormatTime(active.StartTime) : null,
        };
    }

    public async Task<WeeklySummaryDto> GetWeeklySummaryAsync(int employeeId, DateTime weekStart)
    {
        var monday = MondayOf(weekStart);
        var days = new List<DailySummaryDto>();
        for (int i = 0; i < 7; i++)
        {
            days.Add(await GetDailySummaryAsync(employeeId, monday.AddDays(i)));
        }
        var totalMinutes = days.Sum(d => d.TotalMinutes);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        var standardWeekly = (employee?.StandardWorkHoursPerDay ?? 8m) * (employee?.StandardWorkDaysPerWeek ?? 5);
        return new WeeklySummaryDto
        {
            WeekStart = monday,
            Days = days,
            TotalMinutes = totalMinutes,
            TotalHours = Math.Round((decimal)totalMinutes / 60m, 2),
            StandardWeeklyHours = standardWeekly,
        };
    }

    // ============== Helpers ==============

    // Incoming dates from [FromQuery] are Kind=Unspecified; Npgsql rejects those against
    // 'timestamp with time zone' columns. Date-only comparisons need an explicit UTC kind.
    private static DateTime AsUtcDate(DateTime input) =>
        DateTime.SpecifyKind(input.Date, DateTimeKind.Utc);

    private static DateTime MondayOf(DateTime d)
    {
        var date = d.Date;
        int diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff);
    }

    private static string FormatTime(TimeSpan ts) => $"{ts.Hours:D2}:{ts.Minutes:D2}";

    private static TimeLogDto Map(TimeLog l) => new()
    {
        Id = l.Id,
        PublicId = l.PublicId,
        EmployeeId = l.EmployeeId,
        Date = l.Date,
        StartTime = FormatTime(l.StartTime),
        EndTime = l.EndTime.HasValue ? FormatTime(l.EndTime.Value) : null,
        DurationMinutes = l.DurationMinutes,
        Notes = l.Notes,
        CreatedAt = l.CreatedAt,
    };

    // ============== Modification requests ==============

    public async Task<ModificationRequestDto> CreateModificationAsync(int employeeId, CreateModificationRequestDto dto)
    {
        var log = await _context.TimeLogs.FirstOrDefaultAsync(l => l.Id == dto.TimeLogId);
        if (log == null)
            throw new InvalidOperationException("TimeLog not found");
        if (log.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("Cannot request modification for another employee's log");

        var req = new TimeLogModificationRequest
        {
            EmployeeId = employeeId,
            TimeLogId = dto.TimeLogId,
            RequestedStartTime = ParseTime(dto.RequestedStartTime),
            RequestedEndTime = ParseTime(dto.RequestedEndTime),
            Reason = dto.Reason,
            Status = ModificationRequestStatus.Pending,
        };
        _context.Set<TimeLogModificationRequest>().Add(req);
        await _context.SaveChangesAsync();

        var requesterName = await _context.Employees
            .Where(e => e.Id == employeeId)
            .Select(e => e.FirstName + " " + e.LastName)
            .FirstOrDefaultAsync() ?? "An employee";
        await NotifyApproversAsync(employeeId, NotifType.OvertimeDetected,
            "Time log modification requested",
            $"{requesterName} requested a change to a time log dated {log.Date:yyyy-MM-dd}.",
            req.Id);

        return MapMod(req, null);
    }

    public async Task<List<ModificationRequestDto>> GetMyModificationsAsync(int employeeId)
    {
        var rows = await _context.Set<TimeLogModificationRequest>()
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        var names = await BuildNameMapAsync(rows.Select(r => r.EmployeeId));
        return rows.Select(r => MapMod(r, names.GetValueOrDefault(r.EmployeeId))).ToList();
    }

    public async Task<List<ModificationRequestDto>> GetPendingModificationsInScopeAsync(int approverEmployeeId)
    {
        var scope = await _scope.GetScopeEmployeeIdsAsync(approverEmployeeId);
        var rows = await _context.Set<TimeLogModificationRequest>()
            .Where(r => scope.Contains(r.EmployeeId) && r.Status == ModificationRequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
        var names = await BuildNameMapAsync(rows.Select(r => r.EmployeeId));
        return rows.Select(r => MapMod(r, names.GetValueOrDefault(r.EmployeeId))).ToList();
    }

    private async Task<Dictionary<int, string>> BuildNameMapAsync(IEnumerable<int> employeeIds)
    {
        var ids = employeeIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, string>();
        return await _context.Employees
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);
    }

    public async Task<ModificationRequestDto?> ApproveModificationAsync(int requestId, int approverEmployeeId)
    {
        var req = await _context.Set<TimeLogModificationRequest>()
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (req == null) return null;
        if (req.Status != ModificationRequestStatus.Pending)
            throw new InvalidOperationException($"Cannot approve {req.Status} request");

        await _scope.EnsureInScopeAsync(approverEmployeeId, req.EmployeeId);

        // Apply the modification to the underlying TimeLog
        var log = await _context.TimeLogs.FirstAsync(l => l.Id == req.TimeLogId);
        log.StartTime = req.RequestedStartTime;
        log.EndTime = req.RequestedEndTime;
        log.DurationMinutes = (int)(req.RequestedEndTime - req.RequestedStartTime).TotalMinutes;

        req.Status = ModificationRequestStatus.Approved;
        req.ApprovedById = approverEmployeeId;
        req.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyEmployeeAsync(req.EmployeeId, NotifType.OvertimeApproved,
            "Time log modification approved",
            "Your time log modification request has been approved.",
            req.Id);

        return MapMod(req, null);
    }

    public async Task<ModificationRequestDto?> RejectModificationAsync(int requestId, int approverEmployeeId, string reason)
    {
        var req = await _context.Set<TimeLogModificationRequest>()
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (req == null) return null;
        if (req.Status != ModificationRequestStatus.Pending)
            throw new InvalidOperationException($"Cannot reject {req.Status} request");

        req.Status = ModificationRequestStatus.Rejected;
        req.ApprovedById = approverEmployeeId;
        req.ProcessedAt = DateTime.UtcNow;
        // Store the reason in the Reason field appended? Or extend the entity? For now: prepend "[REJECTED] reason: ..." to existing Reason.
        req.Reason = string.IsNullOrEmpty(req.Reason)
            ? $"[REJECTED] {reason}"
            : $"{req.Reason}\n[REJECTED] {reason}";
        await _context.SaveChangesAsync();

        await NotifyEmployeeAsync(req.EmployeeId, NotifType.OvertimeApproved,
            "Time log modification rejected",
            $"Your time log modification request was rejected: {reason}",
            req.Id);

        return MapMod(req, null);
    }

    // ============== Notification helpers ==============

    private async Task NotifyEmployeeAsync(int employeeId, NotifType type, string title, string message, int? entityId)
    {
        try
        {
            var userId = await _context.Users
                .Where(u => u.EmployeeId == employeeId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
            if (userId == null) return;
            await _notification.CreateAsync(userId.Value, type, title, message,
                relatedEntityType: nameof(TimeLogModificationRequest), relatedEntityId: entityId);
        }
        catch { /* best-effort */ }
    }

    private async Task NotifyApproversAsync(int ownerEmployeeId, NotifType type, string title, string message, int? entityId)
    {
        try
        {
            var approvers = await _context.Employees
                .Where(e => e.Id == ownerEmployeeId)
                .Select(e => new
                {
                    TeamLeadId = e.Team != null ? e.Team.TeamLeadId : null,
                    DeptHeadId = e.Department != null ? e.Department.HeadId : null,
                })
                .FirstOrDefaultAsync();
            if (approvers == null) return;
            var approverEmployeeIds = new List<int>();
            if (approvers.TeamLeadId.HasValue) approverEmployeeIds.Add(approvers.TeamLeadId.Value);
            if (approvers.DeptHeadId.HasValue && approvers.DeptHeadId.Value != approvers.TeamLeadId)
                approverEmployeeIds.Add(approvers.DeptHeadId.Value);
            if (approverEmployeeIds.Count == 0) return;
            var userIds = await _context.Users
                .Where(u => u.EmployeeId.HasValue && approverEmployeeIds.Contains(u.EmployeeId.Value))
                .Select(u => u.Id)
                .ToListAsync();
            foreach (var uid in userIds)
                await _notification.CreateAsync(uid, type, title, message,
                    relatedEntityType: nameof(TimeLogModificationRequest), relatedEntityId: entityId);
        }
        catch { /* best-effort */ }
    }

    // ============== Helpers (modification) ==============

    private static TimeSpan ParseTime(string hhmm)
    {
        var parts = hhmm.Split(':');
        return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0);
    }

    private static ModificationRequestDto MapMod(TimeLogModificationRequest r, string? employeeName) => new()
    {
        Id = r.Id,
        PublicId = r.PublicId,
        EmployeeId = r.EmployeeId,
        EmployeeName = employeeName,
        TimeLogId = r.TimeLogId,
        RequestedStartTime = FormatTime(r.RequestedStartTime),
        RequestedEndTime = FormatTime(r.RequestedEndTime),
        Reason = r.Reason,
        Status = r.Status.ToString(),
        ApprovedById = r.ApprovedById,
        ProcessedAt = r.ProcessedAt,
        CreatedAt = r.CreatedAt,
    };
}
