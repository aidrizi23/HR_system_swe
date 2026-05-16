using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Documents;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Documents;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/document-categories")]
[Authorize]
public class DocumentCategoriesController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentCategoriesController(IDocumentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentCategoryDto>>> GetAll()
        => Ok(await _service.ListCategoriesAsync());

    [HttpPost]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<DocumentCategoryDto>> Create([FromBody] CreateDocumentCategoryDto dto)
    {
        var c = await _service.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetAll), null, c);
    }

    [HttpPut("{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<ActionResult<DocumentCategoryDto>> Update(int id, [FromBody] UpdateDocumentCategoryDto dto)
    {
        var c = await _service.UpdateCategoryAsync(id, dto);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpDelete("{id}")]
    [RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var removed = await _service.DeleteCategoryAsync(id);
            return removed ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
