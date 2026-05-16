using System.ComponentModel.DataAnnotations;
using HRSystem.API.Models.Performance;

namespace HRSystem.API.DTOs.Performance;

public class CreateReviewCycleDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public ReviewTargetScope TargetScope { get; set; } = ReviewTargetScope.All;
}
