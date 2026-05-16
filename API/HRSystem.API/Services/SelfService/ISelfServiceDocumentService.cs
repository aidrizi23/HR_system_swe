namespace HRSystem.API.Services.SelfService;

public interface ISelfServiceDocumentService
{
    Task<byte[]?> GenerateEmploymentLetterAsync(int currentEmployeeId);
    Task<byte[]?> GenerateSalaryCertificateAsync(int currentEmployeeId);
}
