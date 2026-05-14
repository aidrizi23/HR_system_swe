using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Overtime;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Overtime;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/overtime")]
[Authorize]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public OvertimeController(IOvertimeService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<OvertimeRecordDto>> Submit([FromBody] CreateOvertimeRequestDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.SubmitManualAsync(employeeId, dto));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<OvertimeRecordDto>>> GetMine()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetMineAsync(employeeId));
    }

    [HttpGet("pending")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<OvertimeRecordDto>>> GetPending()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        return Ok(await _service.GetPendingInScopeAsync(employeeId));
    }

    [HttpGet]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<OvertimeRecordDto>>> List([FromQuery] OvertimeFilterDto filter)
        => Ok(await _service.ListAsync(filter));

    [HttpPost("{id}/recommend")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<OvertimeRecordDto>> Recommend(int id, [FromBody] ProcessOvertimeDto dto)
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
            var r = await _service.RecommendAsync(id, employeeId, dto.Comments);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/approve")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<OvertimeRecordDto>> Approve(int id, [FromBody] ProcessOvertimeDto dto)
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
            var r = await _service.ApproveAsync(id, employeeId, dto.Comments);
            return r == null ? NotFound() : Ok(r);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/reject")]
    [RoleAuthorize(RoleType.TeamLead, RoleType.DepartmentManager, RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<OvertimeRecordDto>> Reject(int id, [FromBody] RejectOvertimeDto dto)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        var isHrActor = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try
        {
            var r = await _service.RejectAsync(id, employeeId, dto.Reason, isHrActor);
            return r == null ? NotFound() : Ok(r);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
