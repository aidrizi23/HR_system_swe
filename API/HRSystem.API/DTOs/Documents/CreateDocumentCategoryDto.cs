using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Documents;

public class CreateDocumentCategoryDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
