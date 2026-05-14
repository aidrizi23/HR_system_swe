using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Holidays;
using HolidayEntity = HRSystem.API.Models.Holidays.Holiday;

namespace HRSystem.API.Services.Holidays;

public class HolidayService : IHolidayService
{
    private readonly AppDbContext _context;

    public HolidayService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayDto>> GetForYearAsync(int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        return await GetExpandedAsync(from, to);
    }

    public async Task<List<HolidayDto>> GetUpcomingAsync(int daysAhead)
    {
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(daysAhead);
        return await GetExpandedAsync(from, to);
    }

    public async Task<HolidayDto?> GetByIdAsync(int id)
    {
        var h = await _context.Holidays.FindAsync(id);
        return h == null ? null : Map(h, h.Date);
    }

    public async Task<HolidayDto> CreateAsync(CreateHolidayDto dto)
    {
        var h = new HolidayEntity
        {
            Name = dto.Name,
            Date = NormalizeToUtcDate(dto.Date),
            IsRecurring = dto.IsRecurring,
            Description = dto.Description,
            Slug = Slugify(dto.Name),
        };
        _context.Holidays.Add(h);
        await _context.SaveChangesAsync();
        return Map(h, h.Date);
    }

    public async Task<HolidayDto?> UpdateAsync(int id, CreateHolidayDto dto)
    {
        var h = await _context.Holidays.FindAsync(id);
        if (h == null) return null;
        h.Name = dto.Name;
        h.Date = NormalizeToUtcDate(dto.Date);
        h.IsRecurring = dto.IsRecurring;
        h.Description = dto.Description;
        h.Slug = Slugify(dto.Name);
        await _context.SaveChangesAsync();
        return Map(h, h.Date);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var h = await _context.Holidays.FindAsync(id);
        if (h == null) return false;
        _context.Holidays.Remove(h);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<HolidayDto>> GetExpandedAsync(DateTime from, DateTime to)
    {
        var rows = await _context.Holidays.ToListAsync();
        var result = new List<HolidayDto>();

        foreach (var h in rows)
        {
            if (!h.IsRecurring)
            {
                if (h.Date >= from.Date && h.Date <= to.Date)
                    result.Add(Map(h, h.Date));
                continue;
            }

            // Recurring: emit one occurrence per year in [from.Year, to.Year]
            for (int year = from.Year; year <= to.Year; year++)
            {
                DateTime? occurrence = null;
                try { occurrence = new DateTime(year, h.Date.Month, h.Date.Day); }
                catch (ArgumentOutOfRangeException) { /* Feb 29 in non-leap year — skip */ }
                if (occurrence != null && occurrence.Value >= from.Date && occurrence.Value <= to.Date)
                    result.Add(Map(h, occurrence.Value));
            }
        }

        return result.OrderBy(h => h.Date).ToList();
    }

    // A holiday is a calendar date, not an instant. Strip time-of-day and stamp Kind=Utc so
    // Npgsql accepts it for 'timestamp with time zone'. Without this, plain "2026-01-01" payloads
    // crash with ArgumentException at SaveChanges.
    private static DateTime NormalizeToUtcDate(DateTime input)
    {
        var date = input.Date;
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }

    private static string Slugify(string s)
    {
        var lower = s.ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        foreach (var c in lower)
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    private static HolidayDto Map(HolidayEntity h, DateTime date) => new()
    {
        Id = h.Id,
        PublicId = h.PublicId,
        Name = h.Name,
        Date = date,                  // expanded occurrence, not h.Date
        IsRecurring = h.IsRecurring,
        Description = h.Description,
        Slug = h.Slug,
        CreatedAt = h.CreatedAt,
    };
}
