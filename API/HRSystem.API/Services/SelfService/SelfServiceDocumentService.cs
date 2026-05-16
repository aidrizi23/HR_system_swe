using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.Services.Pdf;

namespace HRSystem.API.Services.SelfService;

public class SelfServiceDocumentService : ISelfServiceDocumentService
{
    private readonly AppDbContext _context;
    private readonly IPdfTemplateRenderer _pdf;

    public SelfServiceDocumentService(AppDbContext context, IPdfTemplateRenderer pdf)
    {
        _context = context;
        _pdf = pdf;
    }

    public async Task<byte[]?> GenerateEmploymentLetterAsync(int currentEmployeeId)
    {
        var e = await _context.Employees.Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == currentEmployeeId);
        if (e == null) return null;

        var model = new EmploymentLetterModel
        {
            EmployeeName = $"{e.FirstName} {e.LastName}",
            JobTitle = e.JobTitle,
            DepartmentName = e.Department?.Name,
            HireDate = e.HireDate,
            EmploymentStatus = e.Status.ToString(),
            GeneratedAtUtc = DateTime.UtcNow,
        };
        return _pdf.RenderEmploymentLetter(model);
    }

    public async Task<byte[]?> GenerateSalaryCertificateAsync(int currentEmployeeId)
    {
        var e = await _context.Employees.FindAsync(currentEmployeeId);
        if (e == null) return null;

        // Exclude future-dated salary records so a scheduled raise effective next month
        // doesn't leak onto today's certificate.
        var nowUtc = DateTime.UtcNow;
        var salary = await _context.SalaryRecords
            .Where(s => s.EmployeeId == currentEmployeeId && s.EffectiveDate <= nowUtc)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync();
        if (salary == null) return null;

        var allowances = await _context.Allowances
            .Where(a => a.EmployeeId == currentEmployeeId)
            .Select(a => new { a.Type, a.Amount }).ToListAsync();

        var allowancePairs = allowances.Select(a => (a.Type.ToString(), a.Amount)).ToList();
        var totalComp = salary.BaseSalary + allowancePairs.Sum(p => p.Amount);

        var model = new SalaryCertificateModel
        {
            EmployeeName = $"{e.FirstName} {e.LastName}",
            JobTitle = e.JobTitle,
            BaseSalary = salary.BaseSalary,
            Allowances = allowancePairs,
            TotalCompensation = totalComp,
            Currency = salary.Currency,
            GeneratedAtUtc = DateTime.UtcNow,
        };
        return _pdf.RenderSalaryCertificate(model);
    }
}
