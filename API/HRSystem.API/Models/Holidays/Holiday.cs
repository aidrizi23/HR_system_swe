using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Holidays;

public class Holiday : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsRecurring { get; set; }
    public string? Description { get; set; }

    public string Slug { get; set; } = string.Empty;
}
