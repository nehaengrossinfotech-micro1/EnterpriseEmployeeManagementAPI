using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.Models.Entities;
using EnterpriseEmployeeManagementAPI.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class EmployeeRepositoryTests
{
    [Fact]
    public async Task GetAllAsyncIncludesDepartment()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        var result = await repository.GetAllAsync(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Department.Name.Should().Be(employee.Department.Name);
    }

    [Fact]
    public async Task SearchAsyncFindsEmployeeByLastName()
    {
        await using var context = CreateContext();
        await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        var result = await repository.SearchAsync("Morgan", CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Email.Should().Be("alex.morgan@example.com");
    }

    [Fact]
    public async Task DeleteAsyncRemovesEmployee()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        await repository.DeleteAsync(employee, CancellationToken.None);

        (await context.Employees.AnyAsync()).Should().BeFalse();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Employee> SeedEmployeeAsync(ApplicationDbContext context)
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = "EMP-1001",
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex.morgan@example.com",
            JobTitle = "Engineer",
            DepartmentId = department.Id,
            Department = department,
            HireDate = new DateOnly(2024, 1, 15),
            CreatedAtUtc = DateTime.UtcNow,
        };

        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();
        return employee;
    }
}
