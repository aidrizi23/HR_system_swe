namespace HRSystem.API.DTOs.Auth;

public class AuthResponseDto // this is supposed to return a json token. empty for now kur ti vij koha. 
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
