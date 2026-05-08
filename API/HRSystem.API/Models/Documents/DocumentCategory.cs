using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Documents;

public class DocumentCategory : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Slug { get; set; } = string.Empty;
}
