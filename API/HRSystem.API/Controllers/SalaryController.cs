using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Salary;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Salary;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/salary")]
[Authorize]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService _service;
    private readonly ISalaryProjectionService _projection;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public SalaryController(ISalaryService service, ISalaryProjectionService projection,
        ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _projection = projection;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        var s = await _service.GetMineAsync(empId.Value);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpGet("me/projection")]
    public async Task<IActionResult> GetMineProjection()
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        var p = await _projection.ComputeAsync(empId.Value);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpGet("employee/{employeeIdentifier}/projection")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> GetEmployeeProjection(string employeeIdentifier)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        var p = await _projection.ComputeAsync(id.Value);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpGet("employee/{employeeIdentifier}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> GetByEmployee(string employeeIdentifier)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        var s = await _service.GetByEmployeeAsync(id.Value);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpGet("employee/{employeeIdentifier}/history")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> GetHistory(string employeeIdentifier)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        return Ok(await _service.GetHistoryAsync(id.Value));
    }

    [HttpPost("employee/{employeeIdentifier}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> CreateRecord(string employeeIdentifier, [FromBody] CreateSalaryRecordDto dto)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        try { return Ok(await _service.CreateRecordAsync(id.Value, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("employee/{employeeIdentifier}/allowances")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> AddAllowance(string employeeIdentifier, [FromBody] CreateAllowanceDto dto)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        try { return Ok(await _service.AddAllowanceAsync(id.Value, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("employee/{employeeIdentifier}/bonuses")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> AddBonus(string employeeIdentifier, [FromBody] CreateBonusDto dto)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        try { return Ok(await _service.AddBonusAsync(id.Value, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("employee/{employeeIdentifier}/deductions")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> AddDeduction(string employeeIdentifier, [FromBody] CreateDeductionDto dto)
    {
        var id = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (id == null) return NotFound();
        try { return Ok(await _service.AddDeductionAsync(id.Value, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("allowances/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> DeleteAllowance(int id)
        => await _service.RemoveAllowanceAsync(id) ? NoContent() : NotFound();

    [HttpDelete("bonuses/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> DeleteBonus(int id)
        => await _service.RemoveBonusAsync(id) ? NoContent() : NotFound();

    [HttpDelete("deductions/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> DeleteDeduction(int id)
        => await _service.RemoveDeductionAsync(id) ? NoContent() : NotFound();

    private async Task<int?> CurrentEmployeeOrForbidAsync()
    {
        var uid = _currentUser.UserId;
        if (uid == null) return null;
        var empId = await _context.Users.Where(u => u.Id == uid).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        return empId == 0 ? null : empId;
    }

    private async Task<int?> ResolveEmployeeIdAsync(string identifier)
    {
        if (int.TryParse(identifier, out var id))
            return await _context.Employees.AnyAsync(e => e.Id == id) ? id : null;
        if (Guid.TryParse(identifier, out var pub))
            return await _context.Employees.Where(e => e.PublicId == pub)
                .Select(e => (int?)e.Id).FirstOrDefaultAsync();
        return await _context.Employees.Where(e => e.Slug == identifier)
            .Select(e => (int?)e.Id).FirstOrDefaultAsync();
    }
}
