using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Notifications;

public class Notification : BaseEntity
{
    public int RecipientUserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }

    public User RecipientUser { get; set; } = null!;
}
