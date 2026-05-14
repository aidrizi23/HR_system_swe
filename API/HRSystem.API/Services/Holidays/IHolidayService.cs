using HRSystem.API.DTOs.Holidays;

namespace HRSystem.API.Services.Holidays;

public interface IHolidayService
{
    Task<List<HolidayDto>> GetForYearAsync(int year);
    Task<List<HolidayDto>> GetUpcomingAsync(int daysAhead);
    Task<HolidayDto?> GetByIdAsync(int id);
    Task<HolidayDto> CreateAsync(CreateHolidayDto dto);
    Task<HolidayDto?> UpdateAsync(int id, CreateHolidayDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<HolidayDto>> GetExpandedAsync(DateTime from, DateTime to);
}
