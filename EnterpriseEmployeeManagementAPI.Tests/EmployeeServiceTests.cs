using EnterpriseEmployeeManagementAPI.Interfaces;
using EnterpriseEmployeeManagementAPI.Models.DTOs;
using EnterpriseEmployeeManagementAPI.Models.Entities;
using EnterpriseEmployeeManagementAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class EmployeeServiceTests
{
    [Fact]
    public async Task GetByIdAsyncMapsRepositoryEntity()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetByIdAsync(employee.Id, CancellationToken.None))
            .ReturnsAsync(employee);
        var service = new EmployeeService(
            repository.Object,
            NullLogger<EmployeeService>.Instance);

        var result = await service.GetByIdAsync(employee.Id, CancellationToken.None);

        result.Id.Should().Be(employee.Id);
        result.DepartmentName.Should().Be("Engineering");
        result.Email.Should().Be(employee.Email);
        repository.VerifyAll();
    }

    [Fact]
    public async Task CreateAsyncRejectsDuplicateEmail()
    {
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.EmailExistsAsync(
                "sam@example.com",
                null,
                CancellationToken.None))
            .ReturnsAsync(true);
        var service = new EmployeeService(
            repository.Object,
            NullLogger<EmployeeService>.Instance);
        var request = new CreateEmployeeRequest(
            "EMP-2001",
            "Sam",
            "Taylor",
            "sam@example.com",
            "Developer",
            Guid.NewGuid(),
            new DateOnly(2025, 1, 1));

        Func<Task> act = () => service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        repository.VerifyAll();
    }

    [Fact]
    public async Task CreateAsyncPersistsValidEmployee()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
        };
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.EmailExistsAsync(
                "sam@example.com",
                null,
                CancellationToken.None))
            .ReturnsAsync(false);
        repository
            .Setup(item => item.DepartmentExistsAsync(
                department.Id,
                CancellationToken.None))
            .ReturnsAsync(true);
        repository
            .Setup(item => item.AddAsync(
                It.IsAny<Employee>(),
                CancellationToken.None))
            .ReturnsAsync((Employee employee, CancellationToken _) =>
            {
                employee.Department = department;
                return employee;
            });
        var service = new EmployeeService(
            repository.Object,
            NullLogger<EmployeeService>.Instance);
        var request = new CreateEmployeeRequest(
            "EMP-2001",
            "Sam",
            "Taylor",
            "sam@example.com",
            "Developer",
            department.Id,
            new DateOnly(2025, 1, 1));

        var result = await service.CreateAsync(request, CancellationToken.None);

        result.EmployeeNumber.Should().Be("EMP-2001");
        result.DepartmentName.Should().Be("Engineering");
        repository.VerifyAll();
    }

    private static Employee CreateEmployee()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
        };

        return new Employee
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
    }
}
