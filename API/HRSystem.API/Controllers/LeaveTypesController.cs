using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Leave;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Leave;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/leave-types")]
[Authorize]
public class LeaveTypesController : ControllerBase
{
    private readonly ILeaveService _service;

    public LeaveTypesController(ILeaveService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetAll()
        => Ok(await _service.GetTypesAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveTypeDto>> GetById(int id)
    {
        var t = await _service.GetTypeByIdAsync(id);
        return t == null ? NotFound(new { message = $"LeaveType {id} not found" }) : Ok(t);
    }

    [HttpPost]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<LeaveTypeDto>> Create([FromBody] CreateLeaveTypeDto dto)
    {
        var t = await _service.CreateTypeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<LeaveTypeDto>> Update(int id, [FromBody] CreateLeaveTypeDto dto)
    {
        var t = await _service.UpdateTypeAsync(id, dto);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpDelete("{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var removed = await _service.DeleteTypeAsync(id);
            return removed ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
