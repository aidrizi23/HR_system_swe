using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.DTOs.Notifications;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Notifications;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedNotificationsDto>> List([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.ListMineAsync(userId, unreadOnly, page, pageSize));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> UnreadCount()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var count = await _service.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountDto { Count = count });
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var ok = await _service.MarkReadAsync(id, userId);
        return ok ? Ok(new { message = "Marked read" }) : NotFound();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        await _service.MarkAllReadAsync(userId);
        return Ok(new { message = "All marked read" });
    }
}
