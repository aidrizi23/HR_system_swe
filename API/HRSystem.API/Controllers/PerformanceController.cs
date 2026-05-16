using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Performance;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.Performance;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/performance")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService _service;
    private readonly ICurrentUserService _currentUser;

    public PerformanceController(IPerformanceService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Cycles

    [HttpGet("cycles")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> ListCycles() => Ok(await _service.ListCyclesAsync());

    [HttpGet("cycles/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> GetCycle(int id)
    {
        var c = await _service.GetCycleByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost("cycles")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> CreateCycle([FromBody] CreateReviewCycleDto dto)
    {
        try
        {
            var c = await _service.CreateCycleAsync(dto);
            return CreatedAtAction(nameof(GetCycle), new { id = c.Id }, c);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("cycles/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> UpdateCycle(int id, [FromBody] CreateReviewCycleDto dto)
    {
        try
        {
            var c = await _service.UpdateCycleAsync(id, dto);
            return c == null ? NotFound() : Ok(c);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("cycles/{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> DeleteCycle(int id)
    {
        try
        {
            var ok = await _service.DeleteCycleAsync(id);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("cycles/{id}/start")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> StartCycle(int id)
    {
        try
        {
            var c = await _service.StartCycleAsync(id);
            return Ok(c);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // Reviews

    [HttpGet("reviews")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> ListReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _service.ListReviewsAsync(page, pageSize));

    [HttpGet("reviews/my")]
    public async Task<IActionResult> ListMyReviews()
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.ListMyReviewsAsync(uid));
    }

    [HttpGet("reviews/team")]
    public async Task<IActionResult> ListTeamReviews()
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        return Ok(await _service.ListMyTeamReviewsAsync(uid));
    }

    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReview(int id)
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        var r = await _service.GetReviewByIdAsync(id, uid, isHr);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost("reviews/{id}/self-assessment")]
    public async Task<IActionResult> SubmitSelfAssessment(int id, [FromBody] SubmitSelfAssessmentDto dto)
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        try { return Ok(await _service.SubmitSelfAssessmentAsync(id, dto, uid)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("reviews/{id}/manager-review")]
    public async Task<IActionResult> SubmitManagerReview(int id, [FromBody] SubmitManagerReviewDto dto)
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try { return Ok(await _service.SubmitManagerReviewAsync(id, dto, uid, isHr)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    public class FinalizeReviewDto
    {
        public string? HRNotes { get; set; }
        public int? OverallRating { get; set; }
    }

    [HttpPost("reviews/{id}/finalize")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Finalize(int id, [FromBody] FinalizeReviewDto dto)
    {
        try { return Ok(await _service.FinalizeAsync(id, dto.HRNotes, dto.OverallRating)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("reviews/{id}/goals")]
    public async Task<IActionResult> AddGoal(int id, [FromBody] CreateReviewGoalDto dto)
    {
        var uid = _currentUser.UserId ?? throw new InvalidOperationException("Not authenticated");
        var isHr = User.IsInRole(nameof(RoleType.HRManager)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        try { return Ok(await _service.AddGoalAsync(id, dto, uid, isHr)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
