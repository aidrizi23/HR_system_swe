using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Salary;
using HRSystem.API.Models.Leave;
using HRSystem.API.Models.Payroll;
using HRSystem.API.Services.Holidays;

namespace HRSystem.API.Services.Salary;

public class SalaryProjectionService : ISalaryProjectionService
{
    private readonly AppDbContext _context;
    private readonly IHolidayService _holidays;

    public SalaryProjectionService(AppDbContext context, IHolidayService holidays)
    {
        _context = context;
        _holidays = holidays;
    }

    public async Task<SalaryProjectionDto?> ComputeAsync(int employeeId, DateOnly? forDate = null)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return null;

        var today = forDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var monthStartDt = monthStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var monthEndDt = monthEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var salary = await _context.SalaryRecords
            .Where(s => s.EmployeeId == employeeId && s.EffectiveDate <= monthEndDt)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync();

        if (salary == null)
        {
            return new SalaryProjectionDto
            {
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                CurrentMonth = today.Month,
                CurrentYear = today.Year,
            };
        }

        var allowancesTotal = await _context.Allowances
            .Where(a => a.EmployeeId == employeeId).SumAsync(a => (decimal?)a.Amount) ?? 0m;
        var deductionsTotal = await _context.Deductions
            .Where(d => d.EmployeeId == employeeId).SumAsync(d => (decimal?)d.Amount) ?? 0m;

        var holidayDates = await _holidays.GetExpandedAsync(monthStartDt, monthEndDt);
        var holidaySet = holidayDates.Select(h => h.Date.Date).ToHashSet();

        var leaveRows = await _context.LeaveRequests
            .Include(r => r.LeaveType)
            .Where(r => r.EmployeeId == employeeId
                     && r.Status == LeaveRequestStatus.Approved
                     && r.StartDate <= monthEndDt
                     && r.EndDate >= monthStartDt)
            .ToListAsync();
        var paidLeaveDays = leaveRows.Where(l => l.LeaveType != null && l.LeaveType.IsPaid)
            .Sum(l => BusinessDaysInOverlap(l.StartDate, l.EndDate, monthStart, monthEnd, holidaySet));
        var unpaidLeaveDays = leaveRows.Where(l => l.LeaveType != null && !l.LeaveType.IsPaid)
            .Sum(l => BusinessDaysInOverlap(l.StartDate, l.EndDate, monthStart, monthEnd, holidaySet));

        // Count distinct TimeLog dates that fall on a business day (exclude weekends + holidays).
        // dailyRate is per-business-day; counting weekend/holiday logs against it over-reports earnings.
        var todayDt = today.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var workedDates = await _context.TimeLogs
            .Where(t => t.EmployeeId == employeeId && t.Date >= monthStartDt && t.Date <= todayDt)
            .Select(t => t.Date.Date).Distinct().ToListAsync();
        var workedDays = workedDates.Count(d =>
            d.DayOfWeek != DayOfWeek.Saturday
            && d.DayOfWeek != DayOfWeek.Sunday
            && !holidaySet.Contains(d));

        var ytdPayslips = await _context.Payslips
            .Include(p => p.PayrollRun)
            .Where(p => p.EmployeeId == employeeId
                     && p.Status == PayslipStatus.Finalized
                     && p.PayrollRun.Year == today.Year)
            .ToListAsync();

        var businessDaysInMonth = CountBusinessDays(monthStart, monthEnd, holidaySet);
        var dailyRate = businessDaysInMonth > 0 ? salary.BaseSalary / businessDaysInMonth : 0m;
        var hourlyRate = employee.StandardWorkHoursPerDay > 0
            ? dailyRate / employee.StandardWorkHoursPerDay : 0m;

        var monthlyDeductionsTotal = deductionsTotal + unpaidLeaveDays * dailyRate;
        var projectedGross = salary.BaseSalary + allowancesTotal;
        var projectedNet = projectedGross - monthlyDeductionsTotal;
        var earnedToDate = dailyRate * workedDays;

        return new SalaryProjectionDto
        {
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Currency = salary.Currency,
            AnnualBaseSalary = salary.BaseSalary * 12,
            MonthlyBaseSalary = salary.BaseSalary,
            DailyRate = dailyRate,
            HourlyRate = hourlyRate,
            CurrentMonth = today.Month,
            CurrentYear = today.Year,
            BusinessDaysInMonth = businessDaysInMonth,
            BusinessDaysWorkedSoFar = workedDays,
            PaidLeaveDaysThisMonth = paidLeaveDays,
            UnpaidLeaveDaysThisMonth = unpaidLeaveDays,
            EarnedToDateThisMonth = earnedToDate,
            ProjectedMonthlyGross = projectedGross,
            MonthlyAllowances = allowancesTotal,
            MonthlyDeductions = monthlyDeductionsTotal,
            ProjectedMonthlyNet = projectedNet,
            YearToDateGross = ytdPayslips.Sum(p => p.GrossPay),
            YearToDateDeductions = ytdPayslips.Sum(p => p.DeductionsTotal),
            YearToDateNet = ytdPayslips.Sum(p => p.NetPay),
        };
    }

    private static int CountBusinessDays(DateOnly start, DateOnly end, HashSet<DateTime> holidays)
    {
        var count = 0;
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;
            var dt = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).Date;
            if (holidays.Contains(dt)) continue;
            count++;
        }
        return count;
    }

    // Count business-day overlap between a leave range and the target month.
    // Skips weekends and holidays so leave spanning Sat-Mon doesn't deduct 3 days at the daily rate.
    private static int BusinessDaysInOverlap(
        DateTime leaveStart, DateTime leaveEnd,
        DateOnly monthStart, DateOnly monthEnd,
        HashSet<DateTime> holidays)
    {
        var ls = DateOnly.FromDateTime(leaveStart);
        var le = DateOnly.FromDateTime(leaveEnd);
        var from = ls > monthStart ? ls : monthStart;
        var to = le < monthEnd ? le : monthEnd;
        if (to < from) return 0;
        var count = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;
            var dt = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).Date;
            if (holidays.Contains(dt)) continue;
            count++;
        }
        return count;
    }
}
