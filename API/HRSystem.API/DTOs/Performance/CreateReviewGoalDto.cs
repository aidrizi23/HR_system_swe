using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Performance;

public class CreateReviewGoalDto
{
    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
}
