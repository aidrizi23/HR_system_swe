using Microsoft.AspNetCore.Authorization;
using HRSystem.API.Models.Auth;

namespace HRSystem.API.Auth;

public class RoleAuthorizeAttribute : AuthorizeAttribute
{
    public RoleAuthorizeAttribute(params RoleType[] roles)
    {
        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}
