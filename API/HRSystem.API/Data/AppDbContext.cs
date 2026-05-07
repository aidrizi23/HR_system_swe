using System.Linq.Expressions;
using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Common;
using HRSystem.API.Models.Department;
using HRSystem.API.Models.Employee;
using HRSystem.API.Models.Leave;
using HRSystem.API.Models.Overtime;
using HRSystem.API.Models.TimeTracking;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<DepartmentTransferHistory> DepartmentTransferHistories => Set<DepartmentTransferHistory>();

    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<TimeLogModificationRequest> TimeLogModificationRequests => Set<TimeLogModificationRequest>();

    public DbSet<OvertimeRecord> OvertimeRecords => Set<OvertimeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .HasIndex(nameof(BaseEntity.PublicId))
                    .IsUnique();

                modelBuilder.Entity(clrType)
                    .Property(nameof(BaseEntity.PublicId))
                    .HasDefaultValueSql("gen_random_uuid()");
            }

            if (typeof(ISlugEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property(nameof(ISlugEntity.Slug))
                    .HasMaxLength(160);

                modelBuilder.Entity(clrType)
                    .HasIndex(nameof(ISlugEntity.Slug))
                    .IsUnique();
            }

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var body = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.ParentDepartment)
            .WithMany(d => d.SubDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DepartmentTransferHistory>()
            .HasOne(h => h.Employee)
            .WithMany(e => e.TransferHistory)
            .HasForeignKey(h => h.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DepartmentTransferHistory>()
            .HasOne(h => h.FromDepartment)
            .WithMany()
            .HasForeignKey(h => h.FromDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DepartmentTransferHistory>()
            .HasOne(h => h.ToDepartment)
            .WithMany()
            .HasForeignKey(h => h.ToDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Teams)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.TeamLead)
            .WithMany()
            .HasForeignKey(t => t.TeamLeadId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne<Employee>()
            .WithOne(e => e.User)
            .HasForeignKey<User>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeaveBalance>()
            .HasOne(b => b.LeaveType)
            .WithMany()
            .HasForeignKey(b => b.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(r => r.LeaveType)
            .WithMany()
            .HasForeignKey(r => r.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeLogModificationRequest>()
            .HasOne(r => r.TimeLog)
            .WithMany()
            .HasForeignKey(r => r.TimeLogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Employee>()
            .Property(e => e.StandardWorkHoursPerDay)
            .HasPrecision(5, 2);

        modelBuilder.Entity<LeaveBalance>()
            .Property(b => b.TotalDays)
            .HasPrecision(7, 2);
        modelBuilder.Entity<LeaveBalance>()
            .Property(b => b.UsedDays)
            .HasPrecision(7, 2);
        modelBuilder.Entity<LeaveBalance>()
            .Property(b => b.CarriedOverDays)
            .HasPrecision(7, 2);

        modelBuilder.Entity<LeaveRequest>()
            .Property(r => r.TotalDays)
            .HasPrecision(7, 2);
    }
}
