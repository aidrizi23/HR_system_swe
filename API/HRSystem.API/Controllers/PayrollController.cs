using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Payroll;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Payroll;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public PayrollController(IPayrollService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet("runs")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> ListRuns([FromQuery] int? year)
        => Ok(await _service.ListRunsAsync(year));

    [HttpPost("runs")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> CreateRun([FromBody] CreatePayrollRunDto dto)
    {
        try
        {
            var run = await _service.CreateRunAsync(dto.Year, dto.Month);
            return CreatedAtAction(nameof(ListRuns), null, run);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("runs/{id}/finalize")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> FinalizeRun(int id)
    {
        try { return Ok(await _service.FinalizeRunAsync(id)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("runs/{id}/payslips")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> ListPayslipsForRun(int id)
        => Ok(await _service.ListPayslipsForRunAsync(id));

    [HttpPut("payslips/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> UpdatePayslip(int id, [FromBody] UpdatePayslipDto dto)
    {
        try
        {
            var ps = await _service.UpdatePayslipAsync(id, dto);
            return ps == null ? NotFound() : Ok(ps);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("payslips/me")]
    public async Task<IActionResult> ListMyPayslips()
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        return Ok(await _service.ListMyPayslipsAsync(empId.Value));
    }

    [HttpGet("payslips/{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        var result = await _service.DownloadPdfAsync(id, empId.Value, isHr);
        if (result == null) return NotFound();
        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
    }

    private async Task<int?> CurrentEmployeeOrForbidAsync()
    {
        var uid = _currentUser.UserId;
        if (uid == null) return null;
        var empId = await _context.Users.Where(u => u.Id == uid).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        return empId;
    }
}
