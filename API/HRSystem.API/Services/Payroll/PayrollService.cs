using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Payroll;
using HRSystem.API.Models.Employee;
using HRSystem.API.Models.Payroll;
using HRSystem.API.Services.Documents;
using HRSystem.API.Services.Pdf;

namespace HRSystem.API.Services.Payroll;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IPdfTemplateRenderer _pdf;
    private readonly IFileStorage _storage;

    public PayrollService(AppDbContext context, IPdfTemplateRenderer pdf, IFileStorage storage)
    {
        _context = context;
        _pdf = pdf;
        _storage = storage;
    }

    public async Task<List<PayrollRunDto>> ListRunsAsync(int? year)
    {
        var q = _context.PayrollRuns.AsQueryable();
        if (year.HasValue) q = q.Where(r => r.Year == year.Value);
        var rows = await q.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToListAsync();

        var runIds = rows.Select(r => r.Id).ToList();
        var stats = await _context.Payslips
            .Where(p => runIds.Contains(p.PayrollRunId))
            .GroupBy(p => p.PayrollRunId)
            .Select(g => new
            {
                RunId = g.Key,
                Count = g.Count(),
                Gross = g.Sum(p => p.GrossPay),
                Net = g.Sum(p => p.NetPay),
            })
            .ToDictionaryAsync(x => x.RunId, x => x);

        return rows.Select(r =>
        {
            stats.TryGetValue(r.Id, out var s);
            return MapRun(r, s?.Count ?? 0, s?.Gross ?? 0, s?.Net ?? 0);
        }).ToList();
    }

    public async Task<PayrollRunDto> CreateRunAsync(int year, int month)
    {
        var exists = await _context.PayrollRuns.AnyAsync(r => r.Year == year && r.Month == month);
        if (exists) throw new InvalidOperationException($"Payroll run for {year}-{month:00} already exists");

        var run = new PayrollRun { Year = year, Month = month, Status = PayrollRunStatus.Draft };
        _context.PayrollRuns.Add(run);
        await _context.SaveChangesAsync();

        var employees = await _context.Employees
            .Where(e => e.Status == EmploymentStatus.Active && e.TerminationDate == null)
            .ToListAsync();
        var employeeIds = employees.Select(e => e.Id).ToList();

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Bulk-fetch all four datasets once, keyed by EmployeeId, to avoid N+1.
        // Salary cutoff uses monthEnd (not today) so past-month runs use the salary
        // that was effective at month-end, not the one effective now.
        var salaryByEmployee = (await _context.SalaryRecords
            .Where(s => employeeIds.Contains(s.EmployeeId) && s.EffectiveDate <= monthEnd)
            .ToListAsync())
            .GroupBy(s => s.EmployeeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.EffectiveDate).First());

        var allowancesByEmployee = (await _context.Allowances
            .Where(a => employeeIds.Contains(a.EmployeeId))
            .ToListAsync())
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var deductionsByEmployee = (await _context.Deductions
            .Where(d => employeeIds.Contains(d.EmployeeId))
            .ToListAsync())
            .GroupBy(d => d.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

        var bonusesByEmployee = (await _context.Bonuses
            .Where(b => employeeIds.Contains(b.EmployeeId) && b.Date >= monthStart && b.Date <= monthEnd)
            .ToListAsync())
            .GroupBy(b => b.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));

        var payslips = new List<Payslip>();
        foreach (var e in employees)
        {
            salaryByEmployee.TryGetValue(e.Id, out var salary);
            var baseSalary = salary?.BaseSalary ?? 0;
            var allowances = allowancesByEmployee.GetValueOrDefault(e.Id, 0);
            var deductions = deductionsByEmployee.GetValueOrDefault(e.Id, 0);
            var bonuses = bonusesByEmployee.GetValueOrDefault(e.Id, 0);
            var gross = baseSalary + allowances + bonuses;
            payslips.Add(new Payslip
            {
                PayrollRunId = run.Id,
                EmployeeId = e.Id,
                BaseSalary = baseSalary,
                AllowancesTotal = allowances,
                BonusesTotal = bonuses,
                DeductionsTotal = deductions,
                GrossPay = gross,
                NetPay = gross - deductions,
                Currency = salary?.Currency ?? "USD",
                Status = PayslipStatus.Draft,
            });
        }
        _context.Payslips.AddRange(payslips);
        await _context.SaveChangesAsync();
        return MapRun(run, payslips.Count, payslips.Sum(p => p.GrossPay), payslips.Sum(p => p.NetPay));
    }

    public async Task<PayrollRunDto> FinalizeRunAsync(int id)
    {
        var run = await _context.PayrollRuns.FindAsync(id)
            ?? throw new InvalidOperationException($"Run {id} not found");
        if (run.Status != PayrollRunStatus.Draft)
            throw new InvalidOperationException($"Run is already {run.Status}");

        var payslips = await _context.Payslips
            .Include(p => p.Employee).ThenInclude(e => e!.Department)
            .Where(p => p.PayrollRunId == id).ToListAsync();

        var finalizedAt = DateTime.UtcNow;
        var writtenPdfPaths = new List<string>();

        // Wrap the PDF writes + DB updates in a transaction. If anything fails partway
        // through, roll the DB back and delete any PDFs we already wrote so the run can
        // be retried cleanly. Without this, a mid-loop crash leaves the run permanently
        // un-finalizable (Status == Draft check at top would still block retry, but
        // some payslips would have stale PdfFilePath rows on disk-without-DB or vice versa).
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var ps in payslips)
            {
                var model = new PayslipPdfModel
                {
                    EmployeeName = ps.Employee != null ? $"{ps.Employee.FirstName} {ps.Employee.LastName}" : string.Empty,
                    JobTitle = ps.Employee?.JobTitle,
                    DepartmentName = ps.Employee?.Department?.Name,
                    Year = run.Year,
                    Month = run.Month,
                    BaseSalary = ps.BaseSalary,
                    Allowances = ps.AllowancesTotal,
                    Bonuses = ps.BonusesTotal,
                    Deductions = ps.DeductionsTotal,
                    Gross = ps.GrossPay,
                    Net = ps.NetPay,
                    GeneratedAtUtc = finalizedAt,
                };
                var bytes = _pdf.RenderPayslip(model);
                await using var ms = new MemoryStream(bytes);
                var relativePath = await _storage.SaveAsync(ms, ps.EmployeeId, ".pdf");
                writtenPdfPaths.Add(relativePath);
                ps.PdfFilePath = relativePath;
                ps.Status = PayslipStatus.Finalized;
                ps.FinalizedAt = finalizedAt;
            }
            run.Status = PayrollRunStatus.Finalized;
            run.FinalizedAt = finalizedAt;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            foreach (var path in writtenPdfPaths)
            {
                try { await _storage.DeleteAsync(path); } catch { /* best-effort cleanup */ }
            }
            throw;
        }

        return MapRun(run, payslips.Count, payslips.Sum(p => p.GrossPay), payslips.Sum(p => p.NetPay));
    }

    public async Task<List<PayslipDto>> ListPayslipsForRunAsync(int runId)
    {
        var rows = await _context.Payslips
            .Include(p => p.Employee)
            .Include(p => p.PayrollRun)
            .Where(p => p.PayrollRunId == runId)
            .OrderBy(p => p.EmployeeId).ToListAsync();
        return rows.Select(MapPayslip).ToList();
    }

    public async Task<PayslipDto?> UpdatePayslipAsync(int id, UpdatePayslipDto dto)
    {
        var ps = await _context.Payslips
            .Include(p => p.PayrollRun)
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (ps == null) return null;
        if (ps.PayrollRun.Status != PayrollRunStatus.Draft)
            throw new InvalidOperationException("Cannot edit payslip after run is finalized");

        if (dto.BaseSalary.HasValue) ps.BaseSalary = dto.BaseSalary.Value;
        if (dto.AllowancesTotal.HasValue) ps.AllowancesTotal = dto.AllowancesTotal.Value;
        if (dto.BonusesTotal.HasValue) ps.BonusesTotal = dto.BonusesTotal.Value;
        if (dto.DeductionsTotal.HasValue) ps.DeductionsTotal = dto.DeductionsTotal.Value;
        ps.GrossPay = ps.BaseSalary + ps.AllowancesTotal + ps.BonusesTotal;
        ps.NetPay = ps.GrossPay - ps.DeductionsTotal;
        await _context.SaveChangesAsync();
        return MapPayslip(ps);
    }

    public async Task<List<PayslipDto>> ListMyPayslipsAsync(int currentEmployeeId)
    {
        var rows = await _context.Payslips
            .Include(p => p.PayrollRun)
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == currentEmployeeId && p.PayrollRun.Status == PayrollRunStatus.Finalized)
            .OrderByDescending(p => p.PayrollRun.Year).ThenByDescending(p => p.PayrollRun.Month)
            .ToListAsync();
        return rows.Select(MapPayslip).ToList();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadPdfAsync(
        int payslipId, int currentEmployeeId, bool isHrActor)
    {
        var ps = await _context.Payslips
            .Include(p => p.PayrollRun)
            .FirstOrDefaultAsync(p => p.Id == payslipId);
        if (ps == null) return null;
        if (!isHrActor && ps.EmployeeId != currentEmployeeId) return null;
        if (ps.PayrollRun.Status != PayrollRunStatus.Finalized) return null;
        if (string.IsNullOrEmpty(ps.PdfFilePath)) return null;
        var stream = await _storage.OpenAsync(ps.PdfFilePath);
        var fileName = $"payslip-{ps.PayrollRun.Year}-{ps.PayrollRun.Month:00}-{ps.EmployeeId}.pdf";
        return (stream, "application/pdf", fileName);
    }

    private static PayrollRunDto MapRun(PayrollRun r, int count, decimal totalGross, decimal totalNet) => new()
    {
        Id = r.Id,
        PublicId = r.PublicId,
        Year = r.Year,
        Month = r.Month,
        Status = r.Status,
        PayslipCount = count,
        TotalGross = totalGross,
        TotalNet = totalNet,
        FinalizedAt = r.FinalizedAt,
        CreatedAt = r.CreatedAt,
    };

    private static PayslipDto MapPayslip(Payslip p) => new()
    {
        Id = p.Id,
        PublicId = p.PublicId,
        PayrollRunId = p.PayrollRunId,
        Year = p.PayrollRun?.Year ?? 0,
        Month = p.PayrollRun?.Month ?? 0,
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : string.Empty,
        BaseSalary = p.BaseSalary,
        AllowancesTotal = p.AllowancesTotal,
        BonusesTotal = p.BonusesTotal,
        DeductionsTotal = p.DeductionsTotal,
        GrossPay = p.GrossPay,
        NetPay = p.NetPay,
        Currency = p.Currency,
        Status = p.Status,
        PdfFilePath = p.PdfFilePath,
        FinalizedAt = p.FinalizedAt,
        CreatedAt = p.CreatedAt,
    };
}
