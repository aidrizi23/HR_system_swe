using System.ComponentModel.DataAnnotations;
using HRSystem.API.Models.TaskManagement;

namespace HRSystem.API.DTOs.Tasks;

public class UpdateTaskStatusDto
{
    [Required]
    public WorkTaskStatus Status { get; set; }
}
