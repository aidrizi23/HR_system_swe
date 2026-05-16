using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Tasks;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Tasks;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;
    private readonly ICurrentUserService _currentUser;

    public TasksController(ITaskService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] TaskFilterDto filter)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.ListAsync(filter, userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        var task = await _service.GetByIdAsync(id, userId);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateWorkTaskDto dto)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            var created = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkTaskDto dto)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            var updated = await _service.UpdateAsync(id, dto, userId);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            var ok = await _service.DeleteAsync(id, userId);
            return ok ? Ok(new { message = "deleted" }) : NotFound();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            var updated = await _service.UpdateStatusAsync(id, dto.Status, userId);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> ListComments(int id)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.ListCommentsAsync(id, userId));
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateTaskCommentDto dto)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try
        {
            var created = await _service.AddCommentAsync(id, dto, userId);
            return Ok(created);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
