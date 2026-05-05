using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Auth;

public class User : BaseEntity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RoleType Role { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public int? EmployeeId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
