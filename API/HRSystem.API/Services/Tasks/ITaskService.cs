using HRSystem.API.DTOs.Common;
using HRSystem.API.DTOs.Tasks;
using HRSystem.API.Models.TaskManagement;

namespace HRSystem.API.Services.Tasks;

public interface ITaskService
{
    Task<PaginatedResult<WorkTaskDto>> ListAsync(TaskFilterDto filter, int approverUserId);
    Task<WorkTaskDto?> GetByIdAsync(int id, int approverUserId);
    Task<WorkTaskDto> CreateAsync(CreateWorkTaskDto dto, int creatorUserId);
    Task<WorkTaskDto?> UpdateAsync(int id, UpdateWorkTaskDto dto, int actorUserId);
    Task<bool> DeleteAsync(int id, int actorUserId);
    Task<WorkTaskDto?> UpdateStatusAsync(int id, WorkTaskStatus newStatus, int actorUserId);
    Task<List<TaskCommentDto>> ListCommentsAsync(int taskId, int approverUserId);
    Task<TaskCommentDto> AddCommentAsync(int taskId, CreateTaskCommentDto dto, int authorUserId);
}
