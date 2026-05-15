using HRSystem.API.DTOs.Overtime;

namespace HRSystem.API.Services.Overtime;

public interface IOvertimeService
{
    Task<OvertimeRecordDto> SubmitManualAsync(int employeeId, CreateOvertimeRequestDto dto);
    Task AutoDetectAsync(int employeeId, DateTime date, int totalMinutesToday, int sourceTimeLogId);
    Task<List<OvertimeRecordDto>> GetMineAsync(int employeeId);
    Task<List<OvertimeRecordDto>> GetPendingInScopeAsync(int approverEmployeeId, bool isHrActor);
    Task<List<OvertimeRecordDto>> ListAsync(OvertimeFilterDto filter);
    Task<OvertimeRecordDto?> RecommendAsync(int recordId, int recommenderEmployeeId, string? comments, bool isHrActor);
    Task<OvertimeRecordDto?> ApproveAsync(int recordId, int approverEmployeeId, string? comments);
    Task<OvertimeRecordDto?> RejectAsync(int recordId, int actorEmployeeId, string reason, bool isHrActor);
}
