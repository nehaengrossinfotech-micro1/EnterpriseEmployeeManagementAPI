using EnterpriseEmployeeManagementAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementAPI.Data;

public static class SeedData
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (await dbContext.Departments.AnyAsync(cancellationToken))
        {
            return;
        }

        var engineering = new Department
        {
            Id = Guid.Parse("7f7ac456-7a6d-4ed4-92a3-12c61c51aaf4"),
            Name = "Engineering",
            Description = "Product engineering and platform operations",
        };
        var peopleOperations = new Department
        {
            Id = Guid.Parse("8e2f2ae1-b504-4305-8f7d-47e6465b17e0"),
            Name = "People Operations",
            Description = "People, culture, and organizational development",
        };

        await dbContext.Departments.AddRangeAsync(
            [engineering, peopleOperations],
            cancellationToken);
        await dbContext.Employees.AddRangeAsync(
            [
                new Employee
                {
                    Id = Guid.Parse("f08d6f01-45e1-4fc6-8766-14e38e33445d"),
                    EmployeeNumber = "EMP-1001",
                    FirstName = "Aarav",
                    LastName = "Sharma",
                    Email = "aarav.sharma@example.com",
                    JobTitle = "Senior Software Engineer",
                    DepartmentId = engineering.Id,
                    HireDate = new DateOnly(2024, 2, 12),
                    CreatedAtUtc = DateTime.UtcNow,
                },
                new Employee
                {
                    Id = Guid.Parse("c846fb1f-7747-41b7-a11f-7c816a5c95d4"),
                    EmployeeNumber = "EMP-1002",
                    FirstName = "Maya",
                    LastName = "Patel",
                    Email = "maya.patel@example.com",
                    JobTitle = "People Operations Manager",
                    DepartmentId = peopleOperations.Id,
                    HireDate = new DateOnly(2023, 7, 3),
                    CreatedAtUtc = DateTime.UtcNow,
                },
            ],
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
