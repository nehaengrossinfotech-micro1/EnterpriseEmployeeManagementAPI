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

    [Fact]
    public async Task GetByIdAsyncReturnsEmployeeWithDepartment()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        var result = await repository.GetByIdAsync(employee.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Department.Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task AddAsyncPersistsEmployeeAndLoadsDepartment()
    {
        await using var context = CreateContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "People Operations",
        };
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();
        var repository = new EmployeeRepository(context);
        var employee = CreateEmployee(department);

        var result = await repository.AddAsync(employee, CancellationToken.None);

        result.Department.Name.Should().Be(department.Name);
        (await context.Employees.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangedJobTitle()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);
        employee.JobTitle = "Principal Engineer";

        await repository.UpdateAsync(employee, CancellationToken.None);

        (await context.Employees.SingleAsync()).JobTitle.Should().Be("Principal Engineer");
    }

    [Fact]
    public async Task EmailExistsAsyncHonorsExcludedEmployee()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        var exists = await repository.EmailExistsAsync(
            employee.Email,
            null,
            CancellationToken.None);
        var excluded = await repository.EmailExistsAsync(
            employee.Email,
            employee.Id,
            CancellationToken.None);

        exists.Should().BeTrue();
        excluded.Should().BeFalse();
    }

    [Fact]
    public async Task DepartmentExistsAsyncReturnsExpectedResult()
    {
        await using var context = CreateContext();
        var employee = await SeedEmployeeAsync(context);
        var repository = new EmployeeRepository(context);

        (await repository.DepartmentExistsAsync(
            employee.DepartmentId,
            CancellationToken.None)).Should().BeTrue();
        (await repository.DepartmentExistsAsync(
            Guid.NewGuid(),
            CancellationToken.None)).Should().BeFalse();
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

    private static Employee CreateEmployee(Department department)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = "EMP-2001",
            FirstName = "Sam",
            LastName = "Taylor",
            Email = "sam.taylor@example.com",
            JobTitle = "Developer",
            DepartmentId = department.Id,
            HireDate = new DateOnly(2025, 1, 1),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
