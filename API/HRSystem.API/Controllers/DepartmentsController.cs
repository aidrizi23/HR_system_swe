using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.DTOs.Department;
using HRSystem.API.Services.Department;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound(new { message = $"Department with ID {id} not found." });

        return Ok(department);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
    {
        try
        {
            var department = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _departmentService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
