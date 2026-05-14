namespace HRSystem.API.DTOs.Notifications;

public class EmailPreferenceDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsEmailEnabled { get; set; }
}
