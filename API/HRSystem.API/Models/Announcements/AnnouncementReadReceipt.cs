namespace HRSystem.API.Models.Announcements;

public class AnnouncementReadReceipt
{
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime ReadAt { get; set; }

    public Announcement? Announcement { get; set; }
}
