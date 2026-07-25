using EnterpriseEmployeeManagementAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementAPI.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Department>(department =>
        {
            department.HasKey(item => item.Id);
            department.Property(item => item.Name).HasMaxLength(100).IsRequired();
            department.Property(item => item.Description).HasMaxLength(500);
            department.HasIndex(item => item.Name).IsUnique();
        });

        modelBuilder.Entity<Employee>(employee =>
        {
            employee.HasKey(item => item.Id);
            employee.Property(item => item.EmployeeNumber).HasMaxLength(30).IsRequired();
            employee.Property(item => item.FirstName).HasMaxLength(100).IsRequired();
            employee.Property(item => item.LastName).HasMaxLength(100).IsRequired();
            employee.Property(item => item.Email).HasMaxLength(256).IsRequired();
            employee.Property(item => item.JobTitle).HasMaxLength(150).IsRequired();
            employee.Property(item => item.HireDate).HasColumnType("date");
            employee.HasIndex(item => item.EmployeeNumber).IsUnique();
            employee.HasIndex(item => item.Email).IsUnique();
            employee
                .HasOne(item => item.Department)
                .WithMany(item => item.Employees)
                .HasForeignKey(item => item.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
