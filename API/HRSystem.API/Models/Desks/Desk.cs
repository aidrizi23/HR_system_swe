using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Desks;

public class Desk : BaseEntity
{
    public int FloorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DeskStatus Status { get; set; } = DeskStatus.Available;
    public string? Notes { get; set; }

    public Floor Floor { get; set; } = null!;
    public ICollection<DeskBooking> Bookings { get; set; } = new List<DeskBooking>();
}
