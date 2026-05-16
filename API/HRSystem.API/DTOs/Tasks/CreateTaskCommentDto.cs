using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Tasks;

public class CreateTaskCommentDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
