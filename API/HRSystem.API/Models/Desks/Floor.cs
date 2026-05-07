using HRSystem.API.Models.Common;

namespace HRSystem.API.Models.Desks;

public class Floor : BaseEntity
{
    public int OfficeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public FloorStatus Status { get; set; } = FloorStatus.Active;

    public Office Office { get; set; } = null!;
    public ICollection<Desk> Desks { get; set; } = new List<Desk>();
}
