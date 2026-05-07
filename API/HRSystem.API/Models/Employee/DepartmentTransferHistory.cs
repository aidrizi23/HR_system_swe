using HRSystem.API.Models.Common;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Models.Employee;

public class DepartmentTransferHistory : BaseEntity
{
    public int EmployeeId { get; set; }
    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Reason { get; set; }

    public Employee Employee { get; set; } = null!;
    public DepartmentEntity? FromDepartment { get; set; }
    public DepartmentEntity? ToDepartment { get; set; }
}
