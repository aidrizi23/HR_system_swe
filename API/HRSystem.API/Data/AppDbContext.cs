using System.Linq.Expressions;
using HRSystem.API.Models.Announcements;
using HRSystem.API.Models.Audit;
using HRSystem.API.Models.Auth;
using HRSystem.API.Models.Common;
using HRSystem.API.Models.Department;
using HRSystem.API.Models.Desks;
using HRSystem.API.Models.Documents;
using HRSystem.API.Models.Employee;
using HRSystem.API.Models.Holidays;
using HRSystem.API.Models.Leave;
using HRSystem.API.Models.Notifications;
using HRSystem.API.Models.Onboarding;
using HRSystem.API.Models.Overtime;
using HRSystem.API.Models.Payroll;
using HRSystem.API.Models.Performance;
using HRSystem.API.Models.Salary;
using HRSystem.API.Models.TaskManagement;
using HRSystem.API.Models.TimeTracking;
using Microsoft.EntityFrameworkCore;
using EmployeeEntity = HRSystem.API.Models.Employee.Employee;
using DepartmentEntity = HRSystem.API.Models.Department.Department;

namespace HRSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();
    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<DepartmentTransferHistory> DepartmentTransferHistories => Set<DepartmentTransferHistory>();

    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<TimeLogModificationRequest> TimeLogModificationRequests => Set<TimeLogModificationRequest>();

    public DbSet<OvertimeRecord> OvertimeRecords => Set<OvertimeRecord>();

    public DbSet<SalaryRecord> SalaryRecords => Set<SalaryRecord>();
    public DbSet<Allowance> Allowances => Set<Allowance>();
    public DbSet<Bonus> Bonuses => Set<Bonus>();
    public DbSet<Deduction> Deductions => Set<Deduction>();

    public DbSet<DocumentCategory> DocumentCategories => Set<DocumentCategory>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();

    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementReadReceipt> AnnouncementReadReceipts => Set<AnnouncementReadReceipt>();

    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<OnboardingTemplate> OnboardingTemplates => Set<OnboardingTemplate>();
    public DbSet<OnboardingTemplateItem> OnboardingTemplateItems => Set<OnboardingTemplateItem>();
    public DbSet<OnboardingChecklist> OnboardingChecklists => Set<OnboardingChecklist>();
    public DbSet<OnboardingChecklistItem> OnboardingChecklistItems => Set<OnboardingChecklistItem>();

    public DbSet<ReviewCycle> ReviewCycles => Set<ReviewCycle>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<ReviewGoal> ReviewGoals => Set<ReviewGoal>();

    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailPreference> EmailPreferences => Set<EmailPreference>();

    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<DeskBooking> DeskBookings => Set<DeskBooking>();

    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<Payslip> Payslips => Set<Payslip>();

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

        modelBuilder.Entity<EmployeeEntity>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeEntity>()
            .HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeEntity>()
            .HasOne(e => e.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DepartmentEntity>()
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
            .HasOne<EmployeeEntity>()
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

        modelBuilder.Entity<SalaryRecord>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalaryRecord>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(s => s.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Allowance>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bonus>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Deduction>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeDocument>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeDocument>()
            .HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeDocument>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasOne<DepartmentEntity>()
            .WithMany()
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AnnouncementReadReceipt>()
            .HasOne(r => r.Announcement)
            .WithMany(a => a.ReadReceipts)
            .HasForeignKey(r => r.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AnnouncementReadReceipt>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OnboardingTemplateItem>()
            .HasOne(i => i.Template)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OnboardingChecklist>()
            .HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OnboardingChecklist>()
            .HasOne(c => c.Template)
            .WithMany()
            .HasForeignKey(c => c.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OnboardingChecklistItem>()
            .HasOne(i => i.Checklist)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OnboardingChecklistItem>()
            .HasOne(i => i.ResponsibleParty)
            .WithMany()
            .HasForeignKey(i => i.ResponsiblePartyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ReviewCycle>()
            .HasOne(c => c.TargetDepartment)
            .WithMany()
            .HasForeignKey(c => c.TargetDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PerformanceReview>()
            .HasOne(r => r.Cycle)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CycleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PerformanceReview>()
            .HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PerformanceReview>()
            .HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReviewGoal>()
            .HasOne(g => g.Review)
            .WithMany(r => r.Goals)
            .HasForeignKey(g => g.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewGoal>()
            .HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.RecipientUser)
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmailPreference>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmailPreference>()
            .HasIndex(p => new { p.UserId, p.NotificationType })
            .IsUnique();

        modelBuilder.Entity<Floor>()
            .HasOne(f => f.Office)
            .WithMany(o => o.Floors)
            .HasForeignKey(f => f.OfficeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Desk>()
            .HasOne(d => d.Floor)
            .WithMany(f => f.Desks)
            .HasForeignKey(d => d.FloorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeskBooking>()
            .HasOne(b => b.Desk)
            .WithMany(d => d.Bookings)
            .HasForeignKey(b => b.DeskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeskBooking>()
            .HasOne(b => b.Employee)
            .WithMany()
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeskBooking>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(b => b.CancelledById)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DeskBooking>()
            .HasIndex(b => new { b.DeskId, b.BookingDate })
            .IsUnique()
            .HasFilter("\"Status\" = 1");

        modelBuilder.Entity<PayrollRun>()
            .HasOne<EmployeeEntity>()
            .WithMany()
            .HasForeignKey(r => r.FinalizedById)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PayrollRun>()
            .HasIndex(r => new { r.Year, r.Month })
            .IsUnique();

        modelBuilder.Entity<Payslip>()
            .HasOne(p => p.PayrollRun)
            .WithMany(r => r.Payslips)
            .HasForeignKey(p => p.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payslip>()
            .HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeEntity>()
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

        modelBuilder.Entity<SalaryRecord>()
            .Property(s => s.BaseSalary)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Allowance>()
            .Property(a => a.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Bonus>()
            .Property(b => b.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Deduction>()
            .Property(d => d.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payslip>()
            .Property(p => p.BaseSalary)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.AllowancesTotal)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.BonusesTotal)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.DeductionsTotal)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.GrossPay)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.NetPay)
            .HasPrecision(18, 2);

        // One open clock-in session per employee at a time. Backed by a partial unique index
        // so that two concurrent ClockIn requests can't both succeed via check-then-insert.
        modelBuilder.Entity<TimeLog>()
            .HasIndex(t => t.EmployeeId)
            .IsUnique()
            .HasFilter("\"EndTime\" IS NULL")
            .HasDatabaseName("IX_TimeLogs_OpenSession_EmployeeId");

        // One non-terminal AutoDetected overtime record per (Employee, Date). Prevents
        // duplicate Pending/Recommended rows from concurrent ClockOuts on the same shift.
        modelBuilder.Entity<OvertimeRecord>()
            .HasIndex(r => new { r.EmployeeId, r.Date })
            .IsUnique()
            .HasFilter("\"Type\" = 1 AND (\"Status\" = 0 OR \"Status\" = 3)")
            .HasDatabaseName("IX_OvertimeRecords_AutoDetected_Active");
    }
}
