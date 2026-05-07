using System.ComponentModel.DataAnnotations;
using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Leave;

public class LeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedOverDays { get; set; }

    [Timestamp]
    public uint RowVersion { get; set; }

    public LeaveType LeaveType { get; set; } = null!;

    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;
}
