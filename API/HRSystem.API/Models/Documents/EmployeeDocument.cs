using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Documents;

public class EmployeeDocument : BaseEntity
{
    public int EmployeeId { get; set; }
    public int CategoryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int UploadedById { get; set; }
    public string? Notes { get; set; }

    public DocumentCategory? Category { get; set; }
}
