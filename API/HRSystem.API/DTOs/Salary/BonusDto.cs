namespace HRSystem.API.DTOs.Salary;

public class BonusDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime BonusDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsRecurring { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
