using HRSystem.API.DTOs.Payroll;

namespace HRSystem.API.Services.Payroll;

public interface IPayrollService
{
    Task<List<PayrollRunDto>> ListRunsAsync(int? year);
    Task<PayrollRunDto> CreateRunAsync(int year, int month);
    Task<PayrollRunDto> FinalizeRunAsync(int id);
    Task<List<PayslipDto>> ListPayslipsForRunAsync(int runId);
    Task<PayslipDto?> UpdatePayslipAsync(int id, UpdatePayslipDto dto);
    Task<List<PayslipDto>> ListMyPayslipsAsync(int currentEmployeeId);
    Task<(Stream Stream, string ContentType, string FileName)?> DownloadPdfAsync(int payslipId, int currentEmployeeId, bool isHrActor);
}
