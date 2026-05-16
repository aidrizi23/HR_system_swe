using Microsoft.AspNetCore.Http;
using HRSystem.API.DTOs.Documents;

namespace HRSystem.API.Services.Documents;

public interface IDocumentService
{
    // Documents
    Task<EmployeeDocumentDto> UploadAsync(
        int employeeId,
        IFormFile file,
        int categoryId,
        DateTime? expiryDate,
        string? notes,
        int uploadedByEmployeeId);

    Task<List<EmployeeDocumentDto>> ListByEmployeeAsync(int employeeId);

    Task<(Stream Stream, string FileName, string ContentType)?> DownloadAsync(
        int documentId,
        int currentEmployeeId,
        bool isHrActor);

    Task<bool> DeleteAsync(int documentId);

    Task<List<EmployeeDocumentDto>> GetExpiringAsync(int daysAhead);

    // Categories
    Task<List<DocumentCategoryDto>> ListCategoriesAsync();
    Task<DocumentCategoryDto> CreateCategoryAsync(CreateDocumentCategoryDto dto);
    Task<DocumentCategoryDto?> UpdateCategoryAsync(int id, UpdateDocumentCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}
