using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Notifications;
using HRSystem.API.Models.Notifications;
using NotificationEntity = HRSystem.API.Models.Notifications.Notification;

namespace HRSystem.API.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    private const int MaxPageSize = 100;

    public NotificationService(
        AppDbContext context,
        IEmailSender emailSender,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateAsync(
        int recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        var n = new NotificationEntity
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
        };
        _context.Notifications.Add(n);
        await _context.SaveChangesAsync();

        // Email side: consult EmailPreference + send via IEmailSender
        var pref = await _context.EmailPreferences
            .FirstOrDefaultAsync(p => p.UserId == recipientUserId && p.NotificationType == type);
        var emailEnabled = pref?.IsEmailEnabled ?? true;

        if (emailEnabled)
        {
            var email = await _context.Users
                .Where(u => u.Id == recipientUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(email))
            {
                try { await _emailSender.SendAsync(email, title, message); }
                catch (Exception ex) { _logger.LogWarning(ex, "Email send failed for user {UserId}", recipientUserId); }
            }
        }

        return Map(n);
    }

    public async Task<PagedNotificationsDto> ListMineAsync(int userId, bool unreadOnly, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var q = _context.Notifications.AsQueryable()
            .Where(n => n.RecipientUserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);

        var totalCount = await q.CountAsync();
        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedNotificationsDto
        {
            Items = items.Select(Map).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        };
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkReadAsync(int notificationId, int userId)
    {
        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == userId);
        if (n == null) return false;
        if (n.IsRead) return true;
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmailPreferenceDto>> GetPreferencesAsync(int userId)
    {
        var existing = await _context.EmailPreferences
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.NotificationType);

        // Emit one entry per NotificationType, defaulting IsEmailEnabled=true if no row exists
        var result = new List<EmailPreferenceDto>();
        foreach (NotificationType t in Enum.GetValues(typeof(NotificationType)))
        {
            var enabled = existing.TryGetValue(t, out var row) ? row.IsEmailEnabled : true;
            result.Add(new EmailPreferenceDto
            {
                NotificationType = t.ToString(),
                TypeName = t.ToString(),
                IsEmailEnabled = enabled,
            });
        }
        return result;
    }

    public async Task UpdatePreferencesAsync(int userId, List<EmailPreferenceDto> prefs)
    {
        // Validate all incoming type names up front. Previously unknown names were silently
        // skipped and the call returned 200 — callers couldn't distinguish "saved" from "ignored".
        var parsed = new List<(NotificationType Type, bool IsEnabled)>(prefs.Count);
        var unknown = new List<string>();
        foreach (var p in prefs)
        {
            if (Enum.TryParse<NotificationType>(p.NotificationType, ignoreCase: false, out var type)
                && Enum.IsDefined(typeof(NotificationType), type))
            {
                parsed.Add((type, p.IsEmailEnabled));
            }
            else
            {
                unknown.Add(p.NotificationType);
            }
        }
        if (unknown.Count > 0)
            throw new ArgumentException(
                $"Unknown notification type(s): {string.Join(", ", unknown)}");

        for (int attempt = 0; attempt < 2; attempt++)
        {
            var existing = await _context.EmailPreferences
                .Where(p => p.UserId == userId)
                .ToDictionaryAsync(p => p.NotificationType);

            foreach (var (type, isEnabled) in parsed)
            {
                if (existing.TryGetValue(type, out var row))
                {
                    row.IsEmailEnabled = isEnabled;
                }
                else
                {
                    _context.EmailPreferences.Add(new EmailPreference
                    {
                        UserId = userId,
                        NotificationType = type,
                        IsEmailEnabled = isEnabled,
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                // Concurrent insert by another request won the race on the (UserId, NotificationType)
                // unique index. Detach pending entries, re-read, and merge.
                foreach (var entry in _context.ChangeTracker.Entries<EmailPreference>().ToList())
                    entry.State = EntityState.Detached;
            }
        }
    }

    private static NotificationDto Map(NotificationEntity n) => new()
    {
        Id = n.Id,
        PublicId = n.PublicId,
        RecipientUserId = n.RecipientUserId,
        Type = n.Type.ToString(),
        TypeName = n.Type.ToString(),
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        RelatedEntityType = n.RelatedEntityType,
        RelatedEntityId = n.RelatedEntityId,
        CreatedAt = n.CreatedAt,
    };
}
