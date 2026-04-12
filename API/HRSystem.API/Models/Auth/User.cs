using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Auth;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RoleType Role { get; set; }
    public bool IsActive { get; set; } = true;
}
