using HRSystem.API.DTOs.Onboarding;

namespace HRSystem.API.Services.Onboarding;

public interface IOnboardingService
{
    // Templates
    Task<List<OnboardingTemplateDto>> ListTemplatesAsync();
    Task<OnboardingTemplateDto?> GetTemplateByIdAsync(int id);
    Task<OnboardingTemplateDto> CreateTemplateAsync(CreateOnboardingTemplateDto dto);
    Task<OnboardingTemplateDto?> UpdateTemplateAsync(int id, CreateOnboardingTemplateDto dto);
    Task<bool> DeleteTemplateAsync(int id);

    // Checklists
    Task<OnboardingChecklistDto> AssignAsync(int employeeId, int templateId);
    Task<List<OnboardingChecklistDto>> ListChecklistsAsync(int? employeeIdFilter, bool isHrActor);
    Task<OnboardingChecklistDto?> GetChecklistByIdAsync(int id, int currentEmployeeId, bool isHrActor);
    Task<OnboardingChecklistItemDto?> CompleteItemAsync(int itemId, int currentEmployeeId, bool isHrActor);
    Task<List<OnboardingChecklistItemDto>> GetOverdueItemsAsync();
}
