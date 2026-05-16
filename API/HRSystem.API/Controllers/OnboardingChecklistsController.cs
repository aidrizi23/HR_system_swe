using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Auth;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Onboarding;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Onboarding;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
public class OnboardingChecklistsController : ControllerBase
{
    private readonly IOnboardingService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public OnboardingChecklistsController(
        IOnboardingService service,
        ICurrentUserService currentUser,
        AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpPost("assign")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<OnboardingChecklistDto>> Assign([FromBody] AssignChecklistDto dto)
    {
        try
        {
            var c = await _service.AssignAsync(dto.EmployeeId, dto.TemplateId);
            return CreatedAtAction(nameof(GetChecklistById), new { id = c.Id }, c);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("checklists")]
    public async Task<ActionResult<List<OnboardingChecklistDto>>> GetChecklists([FromQuery] int? employeeId)
    {
        var currentEmployeeId = await ResolveCurrentEmployeeIdAsync();
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        var filter = isHr ? employeeId : currentEmployeeId;
        return Ok(await _service.ListChecklistsAsync(filter, isHr));
    }

    [HttpGet("checklists/{id}")]
    public async Task<ActionResult<OnboardingChecklistDto>> GetChecklistById(int id)
    {
        var currentEmployeeId = await ResolveCurrentEmployeeIdAsync();
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try
        {
            var c = await _service.GetChecklistByIdAsync(id, currentEmployeeId, isHr);
            return c == null ? NotFound() : Ok(c);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("checklists/items/{itemIdentifier}/complete")]
    public async Task<ActionResult<OnboardingChecklistItemDto>> CompleteItem(string itemIdentifier)
    {
        var itemId = await ResolveItemIdAsync(itemIdentifier);
        if (itemId == null) return NotFound();

        var currentEmployeeId = await ResolveCurrentEmployeeIdAsync();
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try
        {
            var item = await _service.CompleteItemAsync(itemId.Value, currentEmployeeId, isHr);
            return item == null ? NotFound() : Ok(item);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("overdue")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<List<OnboardingChecklistItemDto>>> GetOverdue()
        => Ok(await _service.GetOverdueItemsAsync());

    // ============== Helpers ==============

    private async Task<int?> ResolveItemIdAsync(string identifier)
    {
        if (int.TryParse(identifier, out var id))
        {
            var exists = await _context.OnboardingChecklistItems.AnyAsync(i => i.Id == id);
            return exists ? id : null;
        }
        if (Guid.TryParse(identifier, out var publicId))
        {
            return await _context.OnboardingChecklistItems
                .Where(i => i.PublicId == publicId)
                .Select(i => (int?)i.Id)
                .FirstOrDefaultAsync();
        }
        return null;
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
