using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Common;
using HRSystem.API.DTOs.Performance;
using HRSystem.API.Models.Employee;
using HRSystem.API.Models.Performance;
using HRSystem.API.Models.Notifications;
using HRSystem.API.Services.Notifications;

namespace HRSystem.API.Services.Performance;

public class PerformanceService : IPerformanceService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notifications;
    private readonly ILogger<PerformanceService> _logger;

    public PerformanceService(AppDbContext context, INotificationService notifications, ILogger<PerformanceService> logger)
    {
        _context = context;
        _notifications = notifications;
        _logger = logger;
    }

    // ===== Cycles =====

    public async Task<List<ReviewCycleDto>> ListCyclesAsync()
    {
        var rows = await _context.ReviewCycles.OrderByDescending(c => c.StartDate).ToListAsync();
        var counts = await _context.PerformanceReviews
            .GroupBy(r => r.CycleId)
            .Select(g => new { CycleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CycleId, x => x.Count);
        return rows.Select(c => MapCycle(c, counts.GetValueOrDefault(c.Id, 0))).ToList();
    }

    public async Task<ReviewCycleDto?> GetCycleByIdAsync(int id)
    {
        var c = await _context.ReviewCycles.FindAsync(id);
        if (c == null) return null;
        var count = await _context.PerformanceReviews.CountAsync(r => r.CycleId == id);
        return MapCycle(c, count);
    }

    public async Task<ReviewCycleDto> CreateCycleAsync(CreateReviewCycleDto dto)
    {
        if (dto.StartDate >= dto.EndDate)
            throw new InvalidOperationException("StartDate must be before EndDate");
        var cycle = new ReviewCycle
        {
            Name = dto.Name,
            StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc),
            Status = ReviewCycleStatus.Draft,
            TargetScope = dto.TargetScope,
        };
        _context.ReviewCycles.Add(cycle);
        await _context.SaveChangesAsync();
        return MapCycle(cycle, 0);
    }

    public async Task<ReviewCycleDto?> UpdateCycleAsync(int id, CreateReviewCycleDto dto)
    {
        var c = await _context.ReviewCycles.FindAsync(id);
        if (c == null) return null;
        if (c.Status != ReviewCycleStatus.Draft)
            throw new InvalidOperationException("Cycle can only be edited while in Draft status");
        c.Name = dto.Name;
        c.StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
        c.EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);
        c.TargetScope = dto.TargetScope;
        await _context.SaveChangesAsync();
        var count = await _context.PerformanceReviews.CountAsync(r => r.CycleId == id);
        return MapCycle(c, count);
    }

    public async Task<bool> DeleteCycleAsync(int id)
    {
        var c = await _context.ReviewCycles.FindAsync(id);
        if (c == null) return false;
        var inUse = await _context.PerformanceReviews.AnyAsync(r => r.CycleId == id);
        if (inUse)
            throw new InvalidOperationException("Cycle is referenced by one or more reviews and cannot be deleted");
        _context.ReviewCycles.Remove(c);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ReviewCycleDto> StartCycleAsync(int id)
    {
        var cycle = await _context.ReviewCycles.FindAsync(id)
            ?? throw new InvalidOperationException($"Cycle {id} not found");
        if (cycle.Status != ReviewCycleStatus.Draft)
            throw new InvalidOperationException("Cycle has already been started");

        var employees = await _context.Employees
            .Where(e => e.Status == EmploymentStatus.Active && e.ManagerId != null)
            .ToListAsync();
        var skipped = await _context.Employees
            .CountAsync(e => e.Status == EmploymentStatus.Active && e.ManagerId == null);
        if (skipped > 0)
            _logger.LogWarning("StartCycle: skipped {Count} active employees with no manager assigned", skipped);

        var reviews = employees.Select(e => new PerformanceReview
        {
            CycleId = cycle.Id,
            EmployeeId = e.Id,
            ManagerId = e.ManagerId!.Value,
            Status = ReviewStatus.PendingSelfAssessment,
        }).ToList();
        _context.PerformanceReviews.AddRange(reviews);

        cycle.Status = ReviewCycleStatus.Active;
        await _context.SaveChangesAsync();
        return MapCycle(cycle, reviews.Count);
    }

    // ===== Reviews =====

    public async Task<PaginatedResult<PerformanceReviewDto>> ListReviewsAsync(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var q = _context.PerformanceReviews
            .Include(r => r.Cycle).Include(r => r.Employee).Include(r => r.Manager)
            .Include(r => r.Goals);
        var total = await q.CountAsync();
        var rows = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedResult<PerformanceReviewDto>
        {
            Items = rows.Select(MapReview).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<List<PerformanceReviewDto>> ListMyReviewsAsync(int currentUserId)
    {
        var employeeId = await ResolveEmployeeIdAsync(currentUserId);
        if (employeeId == null) return new List<PerformanceReviewDto>();
        var rows = await _context.PerformanceReviews
            .Include(r => r.Cycle).Include(r => r.Employee).Include(r => r.Manager).Include(r => r.Goals)
            .Where(r => r.EmployeeId == employeeId.Value)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(MapReview).ToList();
    }

    public async Task<List<PerformanceReviewDto>> ListMyTeamReviewsAsync(int currentUserId)
    {
        var employeeId = await ResolveEmployeeIdAsync(currentUserId);
        if (employeeId == null) return new List<PerformanceReviewDto>();
        var rows = await _context.PerformanceReviews
            .Include(r => r.Cycle).Include(r => r.Employee).Include(r => r.Manager).Include(r => r.Goals)
            .Where(r => r.ManagerId == employeeId.Value)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(MapReview).ToList();
    }

    public async Task<PerformanceReviewDto?> GetReviewByIdAsync(int id, int currentUserId, bool isHrActor)
    {
        var r = await _context.PerformanceReviews
            .Include(x => x.Cycle).Include(x => x.Employee).Include(x => x.Manager).Include(x => x.Goals)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return null;
        if (isHrActor) return MapReview(r);
        var employeeId = await ResolveEmployeeIdAsync(currentUserId);
        if (employeeId == null) return null;
        if (employeeId.Value != r.EmployeeId && employeeId.Value != r.ManagerId) return null;
        return MapReview(r);
    }

    public async Task<PerformanceReviewDto> SubmitSelfAssessmentAsync(int reviewId, SubmitSelfAssessmentDto dto, int currentUserId)
    {
        var r = await _context.PerformanceReviews
            .Include(x => x.Cycle).Include(x => x.Employee).Include(x => x.Manager).Include(x => x.Goals)
            .FirstOrDefaultAsync(x => x.Id == reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} not found");

        var employeeId = await ResolveEmployeeIdAsync(currentUserId);
        if (employeeId == null || employeeId.Value != r.EmployeeId)
            throw new UnauthorizedAccessException("Only the reviewed employee can submit self-assessment");
        if (r.Status != ReviewStatus.PendingSelfAssessment)
            throw new InvalidOperationException($"Review is in status {r.Status}, expected PendingSelfAssessment");

        r.SelfAssessmentText = dto.Text;
        r.SelfAssessmentRating = dto.Rating;
        r.Status = ReviewStatus.PendingManagerReview;
        await _context.SaveChangesAsync();

        await FireReviewPendingAsync(r);
        return MapReview(r);
    }

    public async Task<PerformanceReviewDto> SubmitManagerReviewAsync(int reviewId, SubmitManagerReviewDto dto, int currentUserId, bool isHrActor)
    {
        var r = await _context.PerformanceReviews
            .Include(x => x.Cycle).Include(x => x.Employee).Include(x => x.Manager).Include(x => x.Goals)
            .FirstOrDefaultAsync(x => x.Id == reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} not found");

        if (!isHrActor)
        {
            var employeeId = await ResolveEmployeeIdAsync(currentUserId);
            if (employeeId == null || employeeId.Value != r.ManagerId)
                throw new UnauthorizedAccessException("Only the manager (or HR) can submit manager review");
        }
        if (r.Status != ReviewStatus.PendingManagerReview)
            throw new InvalidOperationException($"Review is in status {r.Status}, expected PendingManagerReview");

        r.ManagerReviewText = dto.Text;
        r.ManagerRating = dto.Rating;
        r.Status = ReviewStatus.PendingHRReview;
        await _context.SaveChangesAsync();
        return MapReview(r);
    }

    public async Task<PerformanceReviewDto> FinalizeAsync(int reviewId, string? hrNotes, int? overallRating)
    {
        var r = await _context.PerformanceReviews
            .Include(x => x.Cycle).Include(x => x.Employee).Include(x => x.Manager).Include(x => x.Goals)
            .FirstOrDefaultAsync(x => x.Id == reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} not found");

        if (r.Status != ReviewStatus.PendingHRReview)
            throw new InvalidOperationException($"Review is in status {r.Status}, expected PendingHRReview");

        r.HRNotes = hrNotes;
        r.OverallRating = overallRating;
        r.Status = ReviewStatus.Completed;
        await _context.SaveChangesAsync();
        return MapReview(r);
    }

    public async Task<ReviewGoalDto> AddGoalAsync(int reviewId, CreateReviewGoalDto dto, int currentUserId, bool isHrActor)
    {
        var r = await _context.PerformanceReviews
            .FirstOrDefaultAsync(x => x.Id == reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} not found");

        if (!isHrActor)
        {
            var employeeId = await ResolveEmployeeIdAsync(currentUserId);
            if (employeeId == null || (employeeId.Value != r.EmployeeId && employeeId.Value != r.ManagerId))
                throw new UnauthorizedAccessException("Only the employee, manager, or HR can add goals");
        }

        var goal = new ReviewGoal
        {
            ReviewId = reviewId,
            EmployeeId = r.EmployeeId,
            Description = dto.Description,
            TargetDate = dto.TargetDate.HasValue
                ? DateTime.SpecifyKind(dto.TargetDate.Value.Date, DateTimeKind.Utc)
                : null,
            Status = GoalStatus.NotStarted,
        };
        _context.ReviewGoals.Add(goal);
        await _context.SaveChangesAsync();
        return MapGoal(goal);
    }

    // ===== Helpers =====

    private async Task<int?> ResolveEmployeeIdAsync(int userId)
    {
        return await _context.Users.Where(u => u.Id == userId)
            .Select(u => u.EmployeeId).FirstOrDefaultAsync();
    }

    private async Task FireReviewPendingAsync(PerformanceReview r)
    {
        try
        {
            var managerUserId = await _context.Users
                .Where(u => u.EmployeeId == r.ManagerId)
                .Select(u => (int?)u.Id).FirstOrDefaultAsync();
            if (managerUserId == null) return;
            var employeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "an employee";
            var cycleName = r.Cycle?.Name ?? "the review cycle";
            await _notifications.CreateAsync(
                managerUserId.Value,
                NotificationType.ReviewPending,
                title: $"Review ready: {employeeName}",
                message: $"{employeeName} has completed their self-assessment for the {cycleName} cycle. Open to enter your review.",
                relatedEntityType: "PerformanceReview",
                relatedEntityId: r.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fire ReviewPending for review {ReviewId}", r.Id);
        }
    }

    private static ReviewCycleDto MapCycle(ReviewCycle c, int reviewCount) => new()
    {
        Id = c.Id, PublicId = c.PublicId, Name = c.Name,
        StartDate = c.StartDate, EndDate = c.EndDate,
        Status = c.Status, TargetScope = c.TargetScope,
        ReviewCount = reviewCount, CreatedAt = c.CreatedAt,
    };

    private static PerformanceReviewDto MapReview(PerformanceReview r) => new()
    {
        Id = r.Id, PublicId = r.PublicId,
        CycleId = r.CycleId, CycleName = r.Cycle?.Name ?? "",
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "",
        ManagerId = r.ManagerId,
        ManagerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : "",
        SelfAssessmentText = r.SelfAssessmentText, SelfAssessmentRating = r.SelfAssessmentRating,
        ManagerReviewText = r.ManagerReviewText, ManagerRating = r.ManagerRating,
        HRNotes = r.HRNotes, OverallRating = r.OverallRating,
        Status = r.Status, CreatedAt = r.CreatedAt,
        Goals = r.Goals?.Select(MapGoal).ToList() ?? new(),
    };

    private static ReviewGoalDto MapGoal(ReviewGoal g) => new()
    {
        Id = g.Id, PublicId = g.PublicId, ReviewId = g.ReviewId,
        EmployeeId = g.EmployeeId,
        Description = g.Description, TargetDate = g.TargetDate,
        Status = g.Status, CarriedForward = g.CarriedForward, CreatedAt = g.CreatedAt,
    };
}
