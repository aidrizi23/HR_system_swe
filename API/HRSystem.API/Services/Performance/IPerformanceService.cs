using HRSystem.API.DTOs.Common;
using HRSystem.API.DTOs.Performance;

namespace HRSystem.API.Services.Performance;

public interface IPerformanceService
{
    Task<List<ReviewCycleDto>> ListCyclesAsync();
    Task<ReviewCycleDto?> GetCycleByIdAsync(int id);
    Task<ReviewCycleDto> CreateCycleAsync(CreateReviewCycleDto dto);
    Task<ReviewCycleDto?> UpdateCycleAsync(int id, CreateReviewCycleDto dto);
    Task<bool> DeleteCycleAsync(int id);
    Task<ReviewCycleDto> StartCycleAsync(int id);

    Task<PaginatedResult<PerformanceReviewDto>> ListReviewsAsync(int page, int pageSize);
    Task<List<PerformanceReviewDto>> ListMyReviewsAsync(int currentUserId);
    Task<List<PerformanceReviewDto>> ListMyTeamReviewsAsync(int currentUserId);
    Task<PerformanceReviewDto?> GetReviewByIdAsync(int id, int currentUserId, bool isHrActor);

    Task<PerformanceReviewDto> SubmitSelfAssessmentAsync(int reviewId, SubmitSelfAssessmentDto dto, int currentUserId);
    Task<PerformanceReviewDto> SubmitManagerReviewAsync(int reviewId, SubmitManagerReviewDto dto, int currentUserId, bool isHrActor);
    Task<PerformanceReviewDto> FinalizeAsync(int reviewId, string? hrNotes, int? overallRating);

    Task<ReviewGoalDto> AddGoalAsync(int reviewId, CreateReviewGoalDto dto, int currentUserId, bool isHrActor);
}
