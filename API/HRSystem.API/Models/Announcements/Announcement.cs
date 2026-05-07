using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Announcements;

public class Announcement : BaseEntity, ISoftDeletable, ISlugEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
    public int? DepartmentId { get; set; }
    public bool IsPinned { get; set; }
    public DateTime PublishDate { get; set; }
    public int AuthorId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<AnnouncementReadReceipt> ReadReceipts { get; set; } = new List<AnnouncementReadReceipt>();
}
