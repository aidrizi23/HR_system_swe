namespace HRSystem.API.DTOs.Salary;

public class DeductionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
}
