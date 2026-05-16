using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Onboarding;

public class AssignChecklistDto
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int TemplateId { get; set; }
}
