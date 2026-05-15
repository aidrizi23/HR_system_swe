using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs.Leave;

public class InitializeBalancesDto
{
    [Range(2020, 2100)]
    public int Year { get; set; }
}
