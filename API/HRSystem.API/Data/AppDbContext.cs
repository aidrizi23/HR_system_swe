using Microsoft.EntityFrameworkCore;
using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Department;
using HRSystem.API.Models.Employee;

namespace HRSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
}
