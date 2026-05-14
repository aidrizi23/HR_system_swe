using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Holidays;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Holidays;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/holidays")]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _service;

    public HolidaysController(IHolidayService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "year" })]
    public async Task<ActionResult<List<HolidayDto>>> GetForYear([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        return Ok(await _service.GetForYearAsync(y));
    }

    [HttpGet("upcoming")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "daysAhead" })]
    public async Task<ActionResult<List<HolidayDto>>> GetUpcoming([FromQuery] int daysAhead = 90)
    {
        return Ok(await _service.GetUpcomingAsync(daysAhead));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<HolidayDto>> GetById(int id)
    {
        var h = await _service.GetByIdAsync(id);
        return h == null ? NotFound() : Ok(h);
    }

    [HttpPost]
    [Authorize]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<HolidayDto>> Create([FromBody] CreateHolidayDto dto)
    {
        var h = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = h.Id }, h);
    }

    [HttpPut("{id}")]
    [Authorize]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<HolidayDto>> Update(int id, [FromBody] CreateHolidayDto dto)
    {
        var h = await _service.UpdateAsync(id, dto);
        return h == null ? NotFound() : Ok(h);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var removed = await _service.DeleteAsync(id);
        return removed ? NoContent() : NotFound();
    }
}
