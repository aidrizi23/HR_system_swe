using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Onboarding;
using HRSystem.API.Models.Notifications;
using HRSystem.API.Models.Onboarding;
using HRSystem.API.Services.Notifications;

namespace HRSystem.API.Services.Onboarding;

public class OnboardingService : IOnboardingService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notifications;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        AppDbContext context,
        INotificationService notifications,
        ILogger<OnboardingService> logger)
    {
        _context = context;
        _notifications = notifications;
        _logger = logger;
    }

    // ============== Templates ==============

    public async Task<List<OnboardingTemplateDto>> ListTemplatesAsync()
    {
        var rows = await _context.OnboardingTemplates
            .Include(t => t.Items)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return rows.Select(MapTemplate).ToList();
    }

    public async Task<OnboardingTemplateDto?> GetTemplateByIdAsync(int id)
    {
        var t = await _context.OnboardingTemplates
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        return t == null ? null : MapTemplate(t);
    }

    public async Task<OnboardingTemplateDto> CreateTemplateAsync(CreateOnboardingTemplateDto dto)
    {
        var t = new OnboardingTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            TargetEmploymentType = dto.TargetEmploymentType,
            Slug = await ToUniqueSlugAsync(dto.Name, existingId: null),
        };
        foreach (var i in dto.Items)
        {
            t.Items.Add(new OnboardingTemplateItem
            {
                Description = i.Description,
                ResponsibleRole = ParseRole(i.ResponsibleRole),
                DefaultDueDays = i.DefaultDueDays,
            });
        }
        _context.OnboardingTemplates.Add(t);
        await _context.SaveChangesAsync();
        return MapTemplate(t);
    }

    public async Task<OnboardingTemplateDto?> UpdateTemplateAsync(int id, CreateOnboardingTemplateDto dto)
    {
        var t = await _context.OnboardingTemplates
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return null;

        t.Name = dto.Name;
        t.Description = dto.Description;
        t.TargetEmploymentType = dto.TargetEmploymentType;
        t.Slug = await ToUniqueSlugAsync(dto.Name, existingId: id);

        // Wholesale items replacement per SPEC_API semantics
        _context.OnboardingTemplateItems.RemoveRange(t.Items);
        t.Items.Clear();
        foreach (var i in dto.Items)
        {
            t.Items.Add(new OnboardingTemplateItem
            {
                Description = i.Description,
                ResponsibleRole = ParseRole(i.ResponsibleRole),
                DefaultDueDays = i.DefaultDueDays,
            });
        }
        await _context.SaveChangesAsync();
        return MapTemplate(t);
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        var t = await _context.OnboardingTemplates
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return false;

        var inUse = await _context.OnboardingChecklists.AnyAsync(c => c.TemplateId == id);
        if (inUse)
            throw new InvalidOperationException(
                "Template is referenced by one or more checklists and cannot be deleted");

        _context.OnboardingTemplateItems.RemoveRange(t.Items);
        _context.OnboardingTemplates.Remove(t);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============== Checklists ==============

    public async Task<OnboardingChecklistDto> AssignAsync(int employeeId, int templateId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found");
        var template = await _context.OnboardingTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == templateId)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        var alreadyInProgress = await _context.OnboardingChecklists
            .AnyAsync(c => c.EmployeeId == employeeId
                        && c.TemplateId == templateId
                        && c.Status == OnboardingChecklistStatus.InProgress);
        if (alreadyInProgress)
            throw new InvalidOperationException(
                $"Employee {employeeId} already has an in-progress checklist for template {templateId}");

        var hireDateUtc = DateTime.SpecifyKind(employee.HireDate.Date, DateTimeKind.Utc);
        var startedAt = DateTime.UtcNow;

        var checklist = new OnboardingChecklist
        {
            EmployeeId = employeeId,
            TemplateId = templateId,
            StartedAt = startedAt,
            Status = OnboardingChecklistStatus.InProgress,
        };
        foreach (var item in template.Items)
        {
            checklist.Items.Add(new OnboardingChecklistItem
            {
                Description = item.Description,
                DueDate = hireDateUtc.AddDays(item.DefaultDueDays),
                Status = OnboardingItemStatus.Pending,
            });
        }
        _context.OnboardingChecklists.Add(checklist);
        await _context.SaveChangesAsync();

        // Fire OnboardingTaskAssigned notification to the employee
        var userId = await _context.Users
            .Where(u => u.EmployeeId == employeeId)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();
        if (userId != null)
        {
            try
            {
                await _notifications.CreateAsync(
                    userId.Value,
                    NotificationType.OnboardingTaskAssigned,
                    title: $"Onboarding checklist assigned: {template.Name}",
                    message: $"You have been assigned {checklist.Items.Count} onboarding task(s). View them in your onboarding page.",
                    relatedEntityType: "OnboardingChecklist",
                    relatedEntityId: checklist.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fire OnboardingTaskAssigned for checklist {ChecklistId}", checklist.Id);
            }
        }

        return await MapChecklistAsync(checklist);
    }

    public async Task<List<OnboardingChecklistDto>> ListChecklistsAsync(int? employeeIdFilter, bool isHrActor)
    {
        var q = _context.OnboardingChecklists
            .Include(c => c.Items)
            .Include(c => c.Template)
            .AsQueryable();

        if (!isHrActor)
        {
            if (employeeIdFilter == null)
                return new List<OnboardingChecklistDto>();
            q = q.Where(c => c.EmployeeId == employeeIdFilter.Value);
        }
        else if (employeeIdFilter != null)
        {
            q = q.Where(c => c.EmployeeId == employeeIdFilter.Value);
        }

        var rows = await q.OrderByDescending(c => c.StartedAt).ToListAsync();
        return await MapChecklistsAsync(rows);
    }

    public async Task<OnboardingChecklistDto?> GetChecklistByIdAsync(int id, int currentEmployeeId, bool isHrActor)
    {
        var c = await _context.OnboardingChecklists
            .Include(x => x.Items)
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return null;
        if (!isHrActor && c.EmployeeId != currentEmployeeId)
            throw new UnauthorizedAccessException("Not authorized to view this checklist");
        return await MapChecklistAsync(c);
    }

    public async Task<OnboardingChecklistItemDto?> CompleteItemAsync(int itemId, int currentEmployeeId, bool isHrActor)
    {
        var item = await _context.OnboardingChecklistItems
            .Include(i => i.Checklist)
            .FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) return null;

        if (!isHrActor && item.Checklist!.EmployeeId != currentEmployeeId)
            throw new UnauthorizedAccessException("Not authorized to complete this item");

        if (item.Status == OnboardingItemStatus.Completed)
            return MapChecklistItem(item);

        item.Status = OnboardingItemStatus.Completed;
        item.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Re-query pending count AFTER the item save so concurrent completions can't
        // both miss the auto-completion transition (each thread sees the other's commit).
        var pendingRemaining = await _context.OnboardingChecklistItems
            .CountAsync(i => i.ChecklistId == item.ChecklistId
                          && i.Status != OnboardingItemStatus.Completed);
        if (pendingRemaining == 0 && item.Checklist!.Status != OnboardingChecklistStatus.Completed)
        {
            item.Checklist.Status = OnboardingChecklistStatus.Completed;
            item.Checklist.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapChecklistItem(item);
    }

    public async Task<List<OnboardingChecklistItemDto>> GetOverdueItemsAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var rows = await _context.OnboardingChecklistItems
            .Where(i => i.Status == OnboardingItemStatus.Pending && i.DueDate < nowUtc)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
        return rows.Select(MapChecklistItem).ToList();
    }

    // ============== Helpers ==============

    private async Task<string> ToUniqueSlugAsync(string name, int? existingId)
    {
        var baseSlug = Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "template";
        var slug = baseSlug;
        var suffix = 2;
        while (await _context.OnboardingTemplates
                   .AnyAsync(t => t.Slug == slug && (existingId == null || t.Id != existingId.Value)))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }
        return slug;
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

    private static ResponsibleRole ParseRole(string role)
    {
        return Enum.TryParse<ResponsibleRole>(role, ignoreCase: true, out var parsed)
            ? parsed
            : ResponsibleRole.Employee;
    }

    private static OnboardingTemplateDto MapTemplate(OnboardingTemplate t) => new()
    {
        Id = t.Id,
        PublicId = t.PublicId,
        Name = t.Name,
        Description = t.Description,
        TargetEmploymentType = t.TargetEmploymentType,
        Slug = t.Slug,
        Items = t.Items.OrderBy(i => i.Id).Select(MapTemplateItem).ToList(),
        CreatedAt = t.CreatedAt,
    };

    private static OnboardingTemplateItemDto MapTemplateItem(OnboardingTemplateItem i) => new()
    {
        Id = i.Id,
        PublicId = i.PublicId,
        Description = i.Description,
        ResponsibleRole = i.ResponsibleRole.ToString(),
        DefaultDueDays = i.DefaultDueDays,
    };

    private async Task<OnboardingChecklistDto> MapChecklistAsync(OnboardingChecklist c)
    {
        var employeeName = await _context.Employees
            .Where(e => e.Id == c.EmployeeId)
            .Select(e => e.FirstName + " " + e.LastName)
            .FirstOrDefaultAsync();
        var templateName = c.Template?.Name ?? await _context.OnboardingTemplates
            .Where(t => t.Id == c.TemplateId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync() ?? "";
        return MapChecklistCore(c, employeeName, templateName);
    }

    private async Task<List<OnboardingChecklistDto>> MapChecklistsAsync(List<OnboardingChecklist> rows)
    {
        if (rows.Count == 0) return new List<OnboardingChecklistDto>();

        var employeeIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
        var names = await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);

        var missingTemplateIds = rows
            .Where(r => r.Template == null)
            .Select(r => r.TemplateId)
            .Distinct()
            .ToList();
        var fallbackTemplateNames = missingTemplateIds.Count == 0
            ? new Dictionary<int, string>()
            : await _context.OnboardingTemplates
                .Where(t => missingTemplateIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Name);

        return rows.Select(c =>
        {
            var tName = c.Template?.Name ?? fallbackTemplateNames.GetValueOrDefault(c.TemplateId, "");
            return MapChecklistCore(c, names.GetValueOrDefault(c.EmployeeId), tName);
        }).ToList();
    }

    private static OnboardingChecklistDto MapChecklistCore(
        OnboardingChecklist c,
        string? employeeName,
        string templateName) => new()
    {
        Id = c.Id,
        PublicId = c.PublicId,
        EmployeeId = c.EmployeeId,
        EmployeeName = employeeName,
        TemplateId = c.TemplateId,
        TemplateName = templateName,
        StartedAt = c.StartedAt,
        CompletedAt = c.CompletedAt,
        Status = c.Status.ToString(),
        TotalItems = c.Items.Count,
        CompletedItems = c.Items.Count(i => i.Status == OnboardingItemStatus.Completed),
        Items = c.Items.OrderBy(i => i.DueDate).Select(MapChecklistItem).ToList(),
    };

    private static OnboardingChecklistItemDto MapChecklistItem(OnboardingChecklistItem i)
    {
        var nowUtc = DateTime.UtcNow;
        var computedStatus = i.Status == OnboardingItemStatus.Pending && i.DueDate < nowUtc
            ? "Overdue"
            : i.Status.ToString();
        return new OnboardingChecklistItemDto
        {
            Id = i.Id,
            PublicId = i.PublicId,
            Description = i.Description,
            ResponsiblePartyId = i.ResponsiblePartyId,
            DueDate = i.DueDate,
            CompletedAt = i.CompletedAt,
            Status = computedStatus,
        };
    }
}
