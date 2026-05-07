using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Desks;

public class Office : BaseEntity, ISlugEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public OfficeStatus Status { get; set; } = OfficeStatus.Active;

    public string Slug { get; set; } = string.Empty;

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
