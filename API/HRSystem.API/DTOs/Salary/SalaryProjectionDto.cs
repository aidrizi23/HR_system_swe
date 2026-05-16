namespace HRSystem.API.DTOs.Salary;

public class SalaryProjectionDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal AnnualBaseSalary { get; set; }
    public decimal MonthlyBaseSalary { get; set; }
    public decimal DailyRate { get; set; }
    public decimal HourlyRate { get; set; }
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public int BusinessDaysInMonth { get; set; }
    public int BusinessDaysWorkedSoFar { get; set; }
    public int PaidLeaveDaysThisMonth { get; set; }
    public int UnpaidLeaveDaysThisMonth { get; set; }
    public decimal EarnedToDateThisMonth { get; set; }
    public decimal ProjectedMonthlyGross { get; set; }
    public decimal MonthlyAllowances { get; set; }
    public decimal MonthlyDeductions { get; set; }
    public decimal ProjectedMonthlyNet { get; set; }
    public decimal YearToDateGross { get; set; }
    public decimal YearToDateDeductions { get; set; }
    public decimal YearToDateNet { get; set; }
}
