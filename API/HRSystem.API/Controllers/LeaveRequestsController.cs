using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Leave;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Leave;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public LeaveRequestsController(ILeaveService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Submit([FromBody] CreateLeaveRequestDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try
        {
            var req = await _service.SubmitAsync(employeeId, dto);
            return CreatedAtAction(nameof(GetMine), null, req);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetMine()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetMineAsync(employeeId));
    }

    [HttpGet("pending")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetPending()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var approverEmployeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetPendingInScopeAsync(approverEmployeeId));
    }

    [HttpPost("{id}/recommend")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<LeaveRequestDto>> Recommend(int id, [FromBody] LeaveDecisionDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var approverEmployeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try
        {
            var r = await _service.RecommendAsync(id, approverEmployeeId, dto.Comments);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/approve")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<LeaveRequestDto>> Approve(int id, [FromBody] LeaveDecisionDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var approverEmployeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try
        {
            var r = await _service.ApproveAsync(id, approverEmployeeId, dto.Comments);
            return r == null ? NotFound() : Ok(r);
        }
        catch (DbUpdateConcurrencyException ex) { return Conflict(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/reject")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<LeaveRequestDto>> Reject(int id, [FromBody] LeaveRejectionDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var actorEmployeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        var isHrActor = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try
        {
            var r = await _service.RejectAsync(id, actorEmployeeId, dto.Reason, isHrActor);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<LeaveRequestDto>> Cancel(int id)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var actorEmployeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        var isHrActor = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try
        {
            var r = await _service.CancelAsync(id, actorEmployeeId, isHrActor);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (DbUpdateConcurrencyException ex) { return Conflict(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
