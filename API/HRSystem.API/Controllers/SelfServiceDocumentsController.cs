using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.Services.Common;
using HRSystem.API.Services.SelfService;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/self-service")]
[Authorize]
public class SelfServiceDocumentsController : ControllerBase
{
    private readonly ISelfServiceDocumentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public SelfServiceDocumentsController(ISelfServiceDocumentService service,
        ICurrentUserService currentUser, AppDbContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet("employment-letter")]
    public async Task<IActionResult> EmploymentLetter()
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        var pdf = await _service.GenerateEmploymentLetterAsync(empId.Value);
        if (pdf == null) return NotFound();
        return File(pdf, "application/pdf", "employment-letter.pdf");
    }

    [HttpGet("salary-certificate")]
    public async Task<IActionResult> SalaryCertificate()
    {
        var empId = await CurrentEmployeeOrForbidAsync();
        if (empId == null) return Forbid();
        var pdf = await _service.GenerateSalaryCertificateAsync(empId.Value);
        if (pdf == null) return NotFound();
        return File(pdf, "application/pdf", "salary-certificate.pdf");
    }

    private async Task<int?> CurrentEmployeeOrForbidAsync()
    {
        var uid = _currentUser.UserId;
        if (uid == null) return null;
        return await _context.Users.Where(u => u.Id == uid).Select(u => u.EmployeeId).FirstOrDefaultAsync();
    }
}
