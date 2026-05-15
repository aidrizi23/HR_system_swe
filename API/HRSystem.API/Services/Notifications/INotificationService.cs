using HRSystem.API.DTOs.Notifications;
using HRSystem.API.Models.Notifications;

namespace HRSystem.API.Services.Notifications;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(
        int recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    Task<PagedNotificationsDto> ListMineAsync(int userId, bool unreadOnly, int page, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkReadAsync(int notificationId, int userId);
    Task MarkAllReadAsync(int userId);

    Task<List<EmailPreferenceDto>> GetPreferencesAsync(int userId);
    Task UpdatePreferencesAsync(int userId, List<EmailPreferenceDto> prefs);
}
