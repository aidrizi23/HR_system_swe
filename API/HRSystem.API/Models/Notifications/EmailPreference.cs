using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Notifications;

public class EmailPreference : BaseEntity
{
    public int UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool IsEmailEnabled { get; set; } = true;

    public User User { get; set; } = null!;
}
