using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.Auth;
using HRSystem.API.DTOs.Onboarding;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services.Onboarding;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/onboarding/templates")]
[Authorize]
[RoleAuthorize(RoleType.HRManager, RoleType.SuperAdmin)]
public class OnboardingTemplatesController : ControllerBase
{
    private readonly IOnboardingService _service;

    public OnboardingTemplatesController(IOnboardingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<OnboardingTemplateDto>>> GetAll()
        => Ok(await _service.ListTemplatesAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<OnboardingTemplateDto>> GetById(int id)
    {
        var t = await _service.GetTemplateByIdAsync(id);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<ActionResult<OnboardingTemplateDto>> Create([FromBody] CreateOnboardingTemplateDto dto)
    {
        var t = await _service.CreateTemplateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OnboardingTemplateDto>> Update(int id, [FromBody] CreateOnboardingTemplateDto dto)
    {
        var t = await _service.UpdateTemplateAsync(id, dto);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var removed = await _service.DeleteTemplateAsync(id);
            return removed ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
