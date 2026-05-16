using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Documents;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Documents;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class EmployeeDocumentsController : ControllerBase
{
    private readonly IDocumentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public EmployeeDocumentsController(IDocumentService service, ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpPost("employee/{employeeIdentifier}/upload")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<EmployeeDocumentDto>> Upload(
        string employeeIdentifier,
        IFormFile file,
        [FromForm] int categoryId,
        [FromForm] DateTime? expiryDate,
        [FromForm] string? notes)
    {
        if (file == null)
            return BadRequest(new { message = "File is required" });

        var employeeId = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (employeeId == null)
            return NotFound(new { message = $"Employee '{employeeIdentifier}' not found" });

        try
        {
            var uploaderEmployeeId = await ResolveCurrentEmployeeIdAsync();
            var doc = await _service.UploadAsync(
                employeeId.Value, file, categoryId, expiryDate, notes, uploaderEmployeeId);
            return CreatedAtAction(nameof(GetByEmployee), new { employeeIdentifier }, doc);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("employee/{employeeIdentifier}")]
    public async Task<ActionResult<List<EmployeeDocumentDto>>> GetByEmployee(string employeeIdentifier)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeIdentifier);
        if (employeeId == null) return NotFound();

        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        int currentEmployeeId;
        try { currentEmployeeId = await ResolveCurrentEmployeeIdAsync(); }
        catch (InvalidOperationException) { return Forbid(); }

        if (!isHr && currentEmployeeId != employeeId.Value)
            return Forbid();

        return Ok(await _service.ListByEmployeeAsync(employeeId.Value));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        int currentEmployeeId;
        try { currentEmployeeId = await ResolveCurrentEmployeeIdAsync(); }
        catch (InvalidOperationException) { return Forbid(); }

        try
        {
            var result = await _service.DownloadAsync(id, currentEmployeeId, isHr);
            if (result == null) return NotFound();
            return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
        }
        // Non-HR callers must not be able to distinguish "doc exists but isn't yours"
        // from "doc doesn't exist" — both become 404 to prevent ID enumeration.
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (FileNotFoundException) { return NotFound(new { message = "File missing from disk" }); }
    }

    [HttpDelete("{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var removed = await _service.DeleteAsync(id);
        return removed ? NoContent() : NotFound();
    }

    [HttpGet("expiring")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<EmployeeDocumentDto>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        if (daysAhead < 1) daysAhead = 30;
        if (daysAhead > 365) daysAhead = 365;
        return Ok(await _service.GetExpiringAsync(daysAhead));
    }

    private async Task<int?> ResolveEmployeeIdAsync(string identifier)
    {
        if (int.TryParse(identifier, out var id))
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == id);
            return exists ? id : null;
        }
        if (Guid.TryParse(identifier, out var publicId))
        {
            return await _context.Employees
                .Where(e => e.PublicId == publicId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }
        return await _context.Employees
            .Where(e => e.Slug == identifier)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<int> ResolveCurrentEmployeeIdAsync()
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Not authenticated");
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Current user has no employee link");
    }
}
