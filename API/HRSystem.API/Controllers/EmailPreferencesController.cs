using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.DTOs.Notifications;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Notifications;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/notifications/email-preferences")]
[Authorize]
public class EmailPreferencesController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public EmailPreferencesController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmailPreferenceDto>>> Get()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.GetPreferencesAsync(userId));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] List<EmailPreferenceDto> prefs)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            await _service.UpdatePreferencesAsync(userId, prefs);
            return Ok(new { message = "Preferences updated" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
