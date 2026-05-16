namespace HRSystem.API.DTOs.Documents;

public class EmployeeDocumentDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int UploadedById { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
