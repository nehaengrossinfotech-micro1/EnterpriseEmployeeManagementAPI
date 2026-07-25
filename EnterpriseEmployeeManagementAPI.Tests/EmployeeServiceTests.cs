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
    public async Task GetAllAsyncMapsAllRepositoryEntities()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetAllAsync(CancellationToken.None))
            .ReturnsAsync([employee]);
        var service = CreateService(repository);

        var result = await service.GetAllAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(employee.Id);
        repository.VerifyAll();
    }

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

    [Fact]
    public async Task SearchAsyncWithBlankQueryReturnsAllEmployees()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetAllAsync(CancellationToken.None))
            .ReturnsAsync([employee]);
        var service = CreateService(repository);

        var result = await service.SearchAsync(" ", CancellationToken.None);

        result.Should().ContainSingle().Which.Email.Should().Be(employee.Email);
        repository.VerifyAll();
    }

    [Fact]
    public async Task SearchAsyncMapsRepositoryMatches()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.SearchAsync("Morgan", CancellationToken.None))
            .ReturnsAsync([employee]);
        var service = CreateService(repository);

        var result = await service.SearchAsync("Morgan", CancellationToken.None);

        result.Should().ContainSingle().Which.LastName.Should().Be("Morgan");
        repository.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsyncThrowsWhenEmployeeDoesNotExist()
    {
        var employeeId = Guid.NewGuid();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetByIdAsync(employeeId, CancellationToken.None))
            .ReturnsAsync((Employee?)null);
        var service = CreateService(repository);

        Func<Task> act = () => service.GetByIdAsync(employeeId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        repository.VerifyAll();
    }

    [Fact]
    public async Task CreateAsyncRejectsMissingDepartment()
    {
        var departmentId = Guid.NewGuid();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.EmailExistsAsync(
                "sam@example.com",
                null,
                CancellationToken.None))
            .ReturnsAsync(false);
        repository
            .Setup(item => item.DepartmentExistsAsync(
                departmentId,
                CancellationToken.None))
            .ReturnsAsync(false);
        var service = CreateService(repository);
        var request = new CreateEmployeeRequest(
            "EMP-2001",
            "Sam",
            "Taylor",
            "sam@example.com",
            "Developer",
            departmentId,
            new DateOnly(2025, 1, 1));

        Func<Task> act = () => service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        repository.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsyncPersistsNormalizedValues()
    {
        var employee = CreateEmployee();
        var newDepartmentId = Guid.NewGuid();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetByIdAsync(employee.Id, CancellationToken.None))
            .ReturnsAsync(employee);
        repository
            .Setup(item => item.EmailExistsAsync(
                "updated@example.com",
                employee.Id,
                CancellationToken.None))
            .ReturnsAsync(false);
        repository
            .Setup(item => item.DepartmentExistsAsync(
                newDepartmentId,
                CancellationToken.None))
            .ReturnsAsync(true);
        repository
            .Setup(item => item.UpdateAsync(employee, CancellationToken.None))
            .Returns(Task.CompletedTask);
        var service = CreateService(repository);
        var request = new UpdateEmployeeRequest(
            " EMP-1002 ",
            " Alex ",
            " Morgan ",
            " UPDATED@EXAMPLE.COM ",
            " Staff Engineer ",
            newDepartmentId,
            new DateOnly(2023, 1, 1),
            false);

        await service.UpdateAsync(employee.Id, request, CancellationToken.None);

        employee.EmployeeNumber.Should().Be("EMP-1002");
        employee.Email.Should().Be("updated@example.com");
        employee.DepartmentId.Should().Be(newDepartmentId);
        employee.IsActive.Should().BeFalse();
        employee.UpdatedAtUtc.Should().NotBeNull();
        repository.VerifyAll();
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingEmployee()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        repository
            .Setup(item => item.GetByIdAsync(employee.Id, CancellationToken.None))
            .ReturnsAsync(employee);
        repository
            .Setup(item => item.DeleteAsync(employee, CancellationToken.None))
            .Returns(Task.CompletedTask);
        var service = CreateService(repository);

        await service.DeleteAsync(employee.Id, CancellationToken.None);

        repository.VerifyAll();
    }

    private static EmployeeService CreateService(Mock<IEmployeeRepository> repository)
    {
        return new EmployeeService(
            repository.Object,
            NullLogger<EmployeeService>.Instance);
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
