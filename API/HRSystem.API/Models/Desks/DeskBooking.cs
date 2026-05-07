using HRSystem.API.Models.Common;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Models.Desks;

public class DeskBooking : BaseEntity
{
    public int DeskId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime BookingDate { get; set; }
    public DeskBookingStatus Status { get; set; } = DeskBookingStatus.Booked;
    public DateTime? CancelledAt { get; set; }
    public int? CancelledById { get; set; }

    public Desk Desk { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
}
