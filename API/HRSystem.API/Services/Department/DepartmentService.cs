using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Department;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Services.Department;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        return await _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                PublicId = d.PublicId,
                Name = d.Name,
                Description = d.Description,
                Code = d.Code,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _context.Departments.FindAsync(id);
        if (d == null) return null;

        return new DepartmentDto
        {
            Id = d.Id,
            PublicId = d.PublicId,
            Name = d.Name,
            Description = d.Description,
            Code = d.Code,
            CreatedAt = d.CreatedAt
        };
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var codeExists = await _context.Departments.AnyAsync(d => d.Code == dto.Code);
        if (codeExists)
            throw new InvalidOperationException($"a department with code '{dto.Code}' already exists.");

        var department = new DepartmentEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            Code = dto.Code
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            Id = department.Id,
            PublicId = department.PublicId,
            Name = department.Name,
            Description = department.Description,
            Code = department.Code,
            CreatedAt = department.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"dep with ID {id} not found.");

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
    }
}
