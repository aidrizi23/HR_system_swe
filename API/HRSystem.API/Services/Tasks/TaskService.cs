using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Common;
using HRSystem.API.DTOs.Tasks;
using HRSystem.API.Models.Auth;
using HRSystem.API.Models.TaskManagement;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Notifications;

namespace HRSystem.API.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IApprovalScopeService _scope;
    private readonly INotificationService _notifications;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext context, IApprovalScopeService scope,
        INotificationService notifications, ILogger<TaskService> logger)
    {
        _context = context;
        _scope = scope;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<PaginatedResult<WorkTaskDto>> ListAsync(TaskFilterDto filter, int approverUserId)
    {
        var approver = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == approverUserId)
            ?? throw new InvalidOperationException("User not found");

        IQueryable<WorkTask> q = _context.WorkTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy);

        var isHr = approver.Role == RoleType.HRManager || approver.Role == RoleType.SuperAdmin;
        if (!isHr)
        {
            if (approver.EmployeeId == null)
                return new PaginatedResult<WorkTaskDto> { Page = filter.Page, PageSize = filter.PageSize };
            var scopeIds = await _scope.GetScopeEmployeeIdsAsync(approver.EmployeeId.Value);
            if (scopeIds.Count == 0)
                return new PaginatedResult<WorkTaskDto> { Page = filter.Page, PageSize = filter.PageSize };
            q = q.Where(t => scopeIds.Contains(t.AssignedToId));
        }

        if (filter.AssignedToId.HasValue) q = q.Where(t => t.AssignedToId == filter.AssignedToId.Value);
        if (filter.Status.HasValue)       q = q.Where(t => t.Status == filter.Status.Value);
        if (filter.Priority.HasValue)     q = q.Where(t => t.Priority == filter.Priority.Value);
        if (filter.DueDateFrom.HasValue)  q = q.Where(t => t.DueDate >= filter.DueDateFrom.Value);
        if (filter.DueDateTo.HasValue)    q = q.Where(t => t.DueDate <= filter.DueDateTo.Value);

        var total = await q.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var rows = await q.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var taskIds = rows.Select(r => r.Id).ToList();
        var commentCounts = taskIds.Count == 0
            ? new Dictionary<int, int>()
            : await _context.TaskComments
                .Where(c => taskIds.Contains(c.TaskId))
                .GroupBy(c => c.TaskId)
                .Select(g => new { TaskId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TaskId, x => x.Count);

        return new PaginatedResult<WorkTaskDto>
        {
            Items = rows.Select(r => Map(r, commentCounts.GetValueOrDefault(r.Id, 0))).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<WorkTaskDto?> GetByIdAsync(int id, int approverUserId)
    {
        var task = await _context.WorkTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;
        if (!await CanSeeAsync(task, approverUserId)) return null;
        var commentCount = await _context.TaskComments.CountAsync(c => c.TaskId == task.Id);
        return Map(task, commentCount);
    }

    public async Task<WorkTaskDto> CreateAsync(CreateWorkTaskDto dto, int creatorUserId)
    {
        var creator = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == creatorUserId)
            ?? throw new InvalidOperationException("User not found");
        if (creator.EmployeeId == null)
            throw new InvalidOperationException("Current user has no employee link");

        var assignee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.AssignedToId)
            ?? throw new InvalidOperationException($"Assignee {dto.AssignedToId} not found");

        var task = new WorkTask
        {
            Title = dto.Title,
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            AssignedById = creator.EmployeeId.Value,
            Priority = dto.Priority,
            Status = WorkTaskStatus.ToDo,
            DueDate = dto.DueDate.HasValue
                ? DateTime.SpecifyKind(dto.DueDate.Value.Date, DateTimeKind.Utc)
                : null,
            Category = dto.Category,
            Tags = dto.Tags,
            Slug = Slugify(dto.Title),
        };
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        await FireTaskAssignedAsync(task, creator);
        return await GetByIdAsync(task.Id, creatorUserId)
            ?? throw new InvalidOperationException("Failed to reload created task");
    }

    public async Task<WorkTaskDto?> UpdateAsync(int id, UpdateWorkTaskDto dto, int actorUserId)
    {
        var task = await _context.WorkTasks
            .Include(t => t.AssignedTo).Include(t => t.AssignedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;
        if (!await CanEditAsync(task, actorUserId)) throw new UnauthorizedAccessException();

        if (dto.Title != null) { task.Title = dto.Title; task.Slug = Slugify(dto.Title); }
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId.Value;
        if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
        if (dto.Status.HasValue)
        {
            task.Status = dto.Status.Value;
            task.CompletedAt = dto.Status.Value == WorkTaskStatus.Done ? DateTime.UtcNow : null;
        }
        if (dto.DueDate.HasValue)
            task.DueDate = DateTime.SpecifyKind(dto.DueDate.Value.Date, DateTimeKind.Utc);
        if (dto.Category != null) task.Category = dto.Category;
        if (dto.Tags != null) task.Tags = dto.Tags;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(task.Id, actorUserId);
    }

    public async Task<bool> DeleteAsync(int id, int actorUserId)
    {
        var task = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return false;
        if (!await CanEditAsync(task, actorUserId)) throw new UnauthorizedAccessException();
        _context.TaskComments.RemoveRange(_context.TaskComments.Where(c => c.TaskId == id));
        _context.WorkTasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<WorkTaskDto?> UpdateStatusAsync(int id, WorkTaskStatus newStatus, int actorUserId)
    {
        var task = await _context.WorkTasks
            .Include(t => t.AssignedTo).Include(t => t.AssignedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId)
            ?? throw new InvalidOperationException("User not found");
        var isHr = actor.Role == RoleType.HRManager || actor.Role == RoleType.SuperAdmin;
        var actorEmployeeId = actor.EmployeeId;
        if (!isHr && actorEmployeeId != task.AssignedToId && actorEmployeeId != task.AssignedById)
            throw new UnauthorizedAccessException();

        task.Status = newStatus;
        task.CompletedAt = newStatus == WorkTaskStatus.Done ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(task.Id, actorUserId);
    }

    public async Task<List<TaskCommentDto>> ListCommentsAsync(int taskId, int approverUserId)
    {
        var task = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return new List<TaskCommentDto>();
        if (!await CanSeeAsync(task, approverUserId)) return new List<TaskCommentDto>();

        var rows = await _context.TaskComments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
        if (rows.Count == 0) return new List<TaskCommentDto>();

        var authorIds = rows.Select(r => r.AuthorId).Distinct().ToList();
        var authorNames = await _context.Employees
            .Where(e => authorIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);

        return rows.Select(c => new TaskCommentDto
        {
            Id = c.Id, PublicId = c.PublicId, TaskId = c.TaskId,
            AuthorId = c.AuthorId, AuthorName = authorNames.GetValueOrDefault(c.AuthorId, "Unknown"),
            Content = c.Content, CreatedAt = c.CreatedAt,
        }).ToList();
    }

    public async Task<TaskCommentDto> AddCommentAsync(int taskId, CreateTaskCommentDto dto, int authorUserId)
    {
        var task = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new InvalidOperationException($"Task {taskId} not found");
        if (!await CanSeeAsync(task, authorUserId)) throw new UnauthorizedAccessException();

        var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorUserId)
            ?? throw new InvalidOperationException("User not found");
        if (author.EmployeeId == null)
            throw new InvalidOperationException("Current user has no employee link");

        var comment = new TaskComment
        {
            TaskId = taskId,
            AuthorId = author.EmployeeId.Value,
            Content = dto.Content,
        };
        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        var authorEmp = await _context.Employees.FindAsync(author.EmployeeId.Value);
        return new TaskCommentDto
        {
            Id = comment.Id, PublicId = comment.PublicId, TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = authorEmp != null ? $"{authorEmp.FirstName} {authorEmp.LastName}" : "Unknown",
            Content = comment.Content, CreatedAt = comment.CreatedAt,
        };
    }

    // ===== Helpers =====

    private async Task<bool> CanSeeAsync(WorkTask task, int approverUserId)
    {
        var approver = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == approverUserId);
        if (approver == null) return false;
        if (approver.Role == RoleType.HRManager || approver.Role == RoleType.SuperAdmin) return true;
        if (approver.EmployeeId == task.AssignedToId || approver.EmployeeId == task.AssignedById) return true;
        if (approver.EmployeeId == null) return false;
        var scopeIds = await _scope.GetScopeEmployeeIdsAsync(approver.EmployeeId.Value);
        return scopeIds.Contains(task.AssignedToId);
    }

    private async Task<bool> CanEditAsync(WorkTask task, int actorUserId)
    {
        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId);
        if (actor == null) return false;
        if (actor.Role == RoleType.HRManager || actor.Role == RoleType.SuperAdmin) return true;
        return actor.EmployeeId == task.AssignedById;
    }

    private async Task FireTaskAssignedAsync(WorkTask task, User creator)
    {
        try
        {
            var assigneeUserId = await _context.Users
                .Where(u => u.EmployeeId == task.AssignedToId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
            if (assigneeUserId == null) return;

            var creatorEmployee = creator.EmployeeId.HasValue
                ? await _context.Employees.FindAsync(creator.EmployeeId.Value)
                : null;
            var assignerName = creatorEmployee != null
                ? $"{creatorEmployee.FirstName} {creatorEmployee.LastName}"
                : creator.Email;
            var due = task.DueDate?.ToString("yyyy-MM-dd") ?? "no deadline";

            await _notifications.CreateAsync(
                assigneeUserId.Value,
                Models.Notifications.NotificationType.TaskAssigned,
                title: $"New task: {task.Title}",
                message: $"Assigned by {assignerName}. Due {due}.",
                relatedEntityType: "WorkTask",
                relatedEntityId: task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fire TaskAssigned for task {TaskId}", task.Id);
        }
    }

    private static WorkTaskDto Map(WorkTask t, int commentCount) => new()
    {
        Id = t.Id, PublicId = t.PublicId, Title = t.Title, Description = t.Description,
        AssignedToId = t.AssignedToId,
        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : "",
        AssignedById = t.AssignedById,
        AssignedByName = t.AssignedBy != null ? $"{t.AssignedBy.FirstName} {t.AssignedBy.LastName}" : "",
        Priority = t.Priority, Status = t.Status, DueDate = t.DueDate,
        Category = t.Category, Tags = t.Tags, CompletedAt = t.CompletedAt,
        Slug = t.Slug, CreatedAt = t.CreatedAt, CommentCount = commentCount,
    };

    private static string Slugify(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in s.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }
}
