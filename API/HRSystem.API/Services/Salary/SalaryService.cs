using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Salary;
using HRSystem.API.Models.Salary;
using HRSystem.API.Services.Common;

namespace HRSystem.API.Services.Salary;

public class SalaryService : ISalaryService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SalaryService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CurrentSalaryDto?> GetMineAsync(int currentEmployeeId)
        => await GetByEmployeeAsync(currentEmployeeId);

    public async Task<CurrentSalaryDto?> GetByEmployeeAsync(int employeeId)
    {
        var record = await GetCurrentRecordAsync(employeeId);
        if (record == null) return null;

        var employee = await _context.Employees.FindAsync(employeeId);
        var nextEffective = await _context.SalaryRecords
            .Where(s => s.EmployeeId == employeeId && s.EffectiveDate > record.EffectiveDate)
            .OrderBy(s => s.EffectiveDate)
            .Select(s => (DateTime?)s.EffectiveDate)
            .FirstOrDefaultAsync();
        var endDate = nextEffective?.AddDays(-1);

        var allowances = await _context.Allowances
            .Where(a => a.EmployeeId == employeeId).ToListAsync();
        var deductions = await _context.Deductions
            .Where(d => d.EmployeeId == employeeId).ToListAsync();

        return new CurrentSalaryDto
        {
            Id = record.Id,
            EmployeeId = employeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "",
            BaseSalary = record.BaseSalary,
            Currency = record.Currency,
            EffectiveDate = record.EffectiveDate,
            EndDate = endDate,
            TotalAllowances = allowances.Sum(a => a.Amount),
            TotalDeductions = deductions.Sum(d => d.Amount),
            Allowances = allowances.Select(a => MapAllowance(a)).ToList(),
            Deductions = deductions.Select(d => MapDeduction(d)).ToList(),
        };
    }

    public async Task<List<SalaryHistoryDto>> GetHistoryAsync(int employeeId)
    {
        var rows = await _context.SalaryRecords
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveDate).ToListAsync();

        var result = new List<SalaryHistoryDto>(rows.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            // Rows are ordered descending; the prior row (i-1) is the next effective record.
            DateTime? endDate = i == 0 ? null : rows[i - 1].EffectiveDate.AddDays(-1);
            result.Add(new SalaryHistoryDto
            {
                Id = r.Id,
                BaseSalary = r.BaseSalary,
                EffectiveDate = r.EffectiveDate,
                EndDate = endDate,
                Reason = r.Notes,
            });
        }
        return result;
    }

    public async Task<CurrentSalaryDto> CreateRecordAsync(int employeeId, CreateSalaryRecordDto dto)
    {
        var employee = await _context.Employees.FindAsync(employeeId)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found");
        var effective = DateTime.SpecifyKind(dto.EffectiveDate.Date, DateTimeKind.Utc);

        var changedById = await ResolveChangedByIdAsync(employeeId);

        var record = new SalaryRecord
        {
            EmployeeId = employeeId,
            BaseSalary = dto.BaseSalary,
            Currency = dto.Currency,
            EffectiveDate = effective,
            Notes = dto.Reason,
            ChangedById = changedById,
        };
        _context.SalaryRecords.Add(record);
        await _context.SaveChangesAsync();
        return (await GetByEmployeeAsync(employeeId))!;
    }

    public async Task<AllowanceDto> AddAllowanceAsync(int employeeId, CreateAllowanceDto dto)
    {
        await EnsureEmployeeExistsAsync(employeeId);
        var a = new Allowance
        {
            EmployeeId = employeeId,
            Type = dto.Type,
            Amount = dto.Amount,
            IsRecurring = dto.IsRecurring,
            EffectiveDate = DateTime.UtcNow,
        };
        _context.Allowances.Add(a);
        await _context.SaveChangesAsync();
        return MapAllowance(a, dto.Notes);
    }

    public async Task<BonusDto> AddBonusAsync(int employeeId, CreateBonusDto dto)
    {
        await EnsureEmployeeExistsAsync(employeeId);
        var b = new Bonus
        {
            EmployeeId = employeeId,
            Amount = dto.Amount,
            Date = DateTime.SpecifyKind(dto.BonusDate.Date, DateTimeKind.Utc),
            IsRecurring = dto.IsRecurring,
            Reason = dto.Reason,
        };
        _context.Bonuses.Add(b);
        await _context.SaveChangesAsync();
        return MapBonus(b);
    }

    public async Task<DeductionDto> AddDeductionAsync(int employeeId, CreateDeductionDto dto)
    {
        await EnsureEmployeeExistsAsync(employeeId);
        var d = new Deduction
        {
            EmployeeId = employeeId,
            Description = dto.Description,
            Amount = dto.Amount,
            IsRecurring = dto.IsRecurring,
            EffectiveDate = DateTime.UtcNow,
        };
        _context.Deductions.Add(d);
        await _context.SaveChangesAsync();
        return MapDeduction(d);
    }

    public async Task<bool> RemoveAllowanceAsync(int id)
    {
        var a = await _context.Allowances.FindAsync(id);
        if (a == null) return false;
        _context.Allowances.Remove(a);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveBonusAsync(int id)
    {
        var b = await _context.Bonuses.FindAsync(id);
        if (b == null) return false;
        _context.Bonuses.Remove(b);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDeductionAsync(int id)
    {
        var d = await _context.Deductions.FindAsync(id);
        if (d == null) return false;
        _context.Deductions.Remove(d);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<SalaryRecord?> GetCurrentRecordAsync(int employeeId)
    {
        var now = DateTime.UtcNow;
        return await _context.SalaryRecords
            .Where(s => s.EmployeeId == employeeId && s.EffectiveDate <= now)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    private async Task EnsureEmployeeExistsAsync(int employeeId)
    {
        var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
        if (!exists) throw new InvalidOperationException($"Employee {employeeId} not found");
    }

    private async Task<int> ResolveChangedByIdAsync(int targetEmployeeId)
    {
        var uid = _currentUser.UserId;
        if (uid != null)
        {
            var actorEmployeeId = await _context.Users
                .Where(u => u.Id == uid)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();
            if (actorEmployeeId != null && actorEmployeeId.Value != 0)
                return actorEmployeeId.Value;
        }
        return targetEmployeeId;
    }

    private static AllowanceDto MapAllowance(Allowance a, string? notes = null) => new()
    {
        Id = a.Id,
        Type = a.Type,
        Amount = a.Amount,
        IsActive = true,
        IsRecurring = a.IsRecurring,
        Notes = notes,
        CreatedAt = a.CreatedAt,
    };

    private static BonusDto MapBonus(Bonus b) => new()
    {
        Id = b.Id,
        Amount = b.Amount,
        BonusDate = b.Date,
        IsActive = true,
        IsRecurring = b.IsRecurring,
        Reason = b.Reason,
        CreatedAt = b.CreatedAt,
    };

    private static DeductionDto MapDeduction(Deduction d) => new()
    {
        Id = d.Id,
        Description = d.Description,
        Amount = d.Amount,
        IsActive = true,
        IsRecurring = d.IsRecurring,
        CreatedAt = d.CreatedAt,
    };
}
