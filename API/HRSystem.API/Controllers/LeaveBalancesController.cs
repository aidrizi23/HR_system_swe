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
[Route("api/leave-balances")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public LeaveBalancesController(ILeaveService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<LeaveBalanceDto>>> GetMine([FromQuery] int? year)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated");
        var employeeId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
        var y = year ?? DateTime.UtcNow.Year;
        return Ok(await _service.GetMyBalancesAsync(employeeId, y));
    }

    [HttpGet("employee/{employeeId}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<LeaveBalanceDto>>> GetForEmployee(int employeeId, [FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        return Ok(await _service.GetEmployeeBalancesAsync(employeeId, y));
    }

    [HttpPost("initialize")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Initialize([FromBody] InitializeBalancesDto dto)
    {
        await _service.InitializeBalancesAsync(dto.Year);
        return NoContent();
    }
}
