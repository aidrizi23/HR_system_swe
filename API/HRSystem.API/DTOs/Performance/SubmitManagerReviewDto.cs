using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Performance;

public class SubmitManagerReviewDto
{
    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;
    [Range(1, 5)]
    public int? Rating { get; set; }
}
