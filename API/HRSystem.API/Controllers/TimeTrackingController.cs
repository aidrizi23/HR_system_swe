using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.TimeTracking;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.TimeTracking;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/time-tracking")]
[Authorize]
public class TimeTrackingController : ControllerBase
{
    private readonly ITimeTrackingService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public TimeTrackingController(ITimeTrackingService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<TimeLogDto>> ClockIn()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try { return Ok(await _service.ClockInAsync(employeeId)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<TimeLogDto>> ClockOut()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        var r = await _service.ClockOutAsync(employeeId);
        return r == null ? NotFound(new { message = "No open session" }) : Ok(r);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<TimeLogDto>>> GetMine([FromQuery] DateTime? date)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetMineAsync(employeeId, date));
    }

    [HttpGet("team")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<TimeLogDto>>> GetTeam([FromQuery] DateTime date)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetTeamAsync(employeeId, date));
    }

    [HttpGet("summary/daily/mine")]
    public async Task<ActionResult<DailySummaryDto>> GetDaily([FromQuery] DateTime date)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetDailySummaryAsync(employeeId, date));
    }

    [HttpGet("summary/weekly/mine")]
    public async Task<ActionResult<WeeklySummaryDto>> GetWeekly([FromQuery] DateTime weekStart)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetWeeklySummaryAsync(employeeId, weekStart));
    }

    [HttpPost("modifications")]
    public async Task<ActionResult<ModificationRequestDto>> CreateModification([FromBody] CreateModificationRequestDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try { return Ok(await _service.CreateModificationAsync(employeeId, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("modifications/mine")]
    public async Task<ActionResult<List<ModificationRequestDto>>> GetMyMods()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetMyModificationsAsync(employeeId));
    }

    [HttpGet("modifications/pending")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<ModificationRequestDto>>> GetPendingMods()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetPendingModificationsInScopeAsync(employeeId));
    }

    [HttpPost("modifications/{id}/approve")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<ModificationRequestDto>> ApproveMod(int id)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try
        {
            var r = await _service.ApproveModificationAsync(id, employeeId);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("modifications/{id}/reject")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<ModificationRequestDto>> RejectMod(int id, [FromBody] ProcessModificationDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        try
        {
            var r = await _service.RejectModificationAsync(id, employeeId, dto.Reason ?? "");
            return r == null ? NotFound() : Ok(r);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
