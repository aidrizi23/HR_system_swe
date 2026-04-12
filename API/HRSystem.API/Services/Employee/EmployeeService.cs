using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Employee;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;

namespace HRSystem.API.Services.Employee;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.LastName)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                PublicId = e.PublicId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                JobTitle = e.JobTitle,
                HireDate = e.HireDate,
                DepartmentId = e.DepartmentId,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await _context.Employees.FindAsync(id);
        if (e == null) return null;

        return new EmployeeDto
        {
            Id = e.Id,
            PublicId = e.PublicId,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            JobTitle = e.JobTitle,
            HireDate = e.HireDate,
            DepartmentId = e.DepartmentId,
            CreatedAt = e.CreatedAt
        };
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var emailExists = await _context.Employees.AnyAsync(e => e.Email == dto.Email);
        if (emailExists)
            throw new InvalidOperationException($"An employee with email '{dto.Email}' already exists.");

        var employee = new EmployeeEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            JobTitle = dto.JobTitle,
            HireDate = dto.HireDate,
            DepartmentId = dto.DepartmentId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return new EmployeeDto
        {
            Id = employee.Id,
            PublicId = employee.PublicId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            JobTitle = employee.JobTitle,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId,
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new KeyNotFoundException($"Employee with ID {id} not found.");

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }
}
