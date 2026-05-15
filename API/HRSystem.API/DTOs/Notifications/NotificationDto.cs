namespace HRSystem.API.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int RecipientUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedNotificationsDto
{
    public List<NotificationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
