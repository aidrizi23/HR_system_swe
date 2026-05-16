namespace HRSystem.API.DTOs.Tasks;

public class TaskCommentDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int TaskId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
