namespace HRSystem.API.DTOs.Auth;

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
}
