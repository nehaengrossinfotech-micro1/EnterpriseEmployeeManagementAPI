using EnterpriseEmployeeManagementAPI.Controllers;
using EnterpriseEmployeeManagementAPI.Interfaces;
using EnterpriseEmployeeManagementAPI.Models.DTOs;
using EnterpriseEmployeeManagementAPI.Validators;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class EmployeesControllerTests
{
    [Fact]
    public async Task GetAllReturnsEmployees()
    {
        var employees = new[]
        {
            CreateEmployeeDto(),
        };
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(employees);
        var controller = CreateController(service.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(employees);
        service.VerifyAll();
    }

    [Fact]
    public async Task CreateReturnsCreatedAtActionForValidRequest()
    {
        var employee = CreateEmployeeDto();
        var request = new CreateEmployeeRequest(
            employee.EmployeeNumber,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.JobTitle,
            employee.DepartmentId,
            employee.HireDate);
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.CreateAsync(request, CancellationToken.None))
            .ReturnsAsync(employee);
        var controller = CreateController(service.Object);

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(EmployeesController.GetById));
        created.Value.Should().Be(employee);
        service.VerifyAll();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemForInvalidRequest()
    {
        var request = new CreateEmployeeRequest(
            string.Empty,
            string.Empty,
            string.Empty,
            "not-an-email",
            string.Empty,
            Guid.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        var controller = CreateController(service.Object);

        var result = await controller.Create(request, CancellationToken.None);

        var problem = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        problem.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteReturnsNoContent()
    {
        var employeeId = Guid.NewGuid();
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.DeleteAsync(employeeId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        var controller = CreateController(service.Object);

        var result = await controller.Delete(employeeId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        service.VerifyAll();
    }

    [Fact]
    public async Task GetByIdReturnsEmployee()
    {
        var employee = CreateEmployeeDto();
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.GetByIdAsync(employee.Id, CancellationToken.None))
            .ReturnsAsync(employee);
        var controller = CreateController(service.Object);

        var result = await controller.GetById(employee.Id, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(employee);
        service.VerifyAll();
    }

    [Fact]
    public async Task SearchReturnsMatches()
    {
        var employee = CreateEmployeeDto();
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.SearchAsync("Morgan", CancellationToken.None))
            .ReturnsAsync([employee]);
        var controller = CreateController(service.Object);

        var result = await controller.Search("Morgan", CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new[] { employee });
        service.VerifyAll();
    }

    [Fact]
    public async Task UpdateReturnsNoContentForValidRequest()
    {
        var employee = CreateEmployeeDto();
        var request = new UpdateEmployeeRequest(
            employee.EmployeeNumber,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.JobTitle,
            employee.DepartmentId,
            employee.HireDate,
            employee.IsActive);
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        service
            .Setup(item => item.UpdateAsync(
                employee.Id,
                request,
                CancellationToken.None))
            .Returns(Task.CompletedTask);
        var controller = CreateController(service.Object);

        var result = await controller.Update(
            employee.Id,
            request,
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        service.VerifyAll();
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemForInvalidRequest()
    {
        var request = new UpdateEmployeeRequest(
            string.Empty,
            string.Empty,
            string.Empty,
            "not-an-email",
            string.Empty,
            Guid.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            true);
        var service = new Mock<IEmployeeService>(MockBehavior.Strict);
        var controller = CreateController(service.Object);

        var result = await controller.Update(
            Guid.NewGuid(),
            request,
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        service.VerifyNoOtherCalls();
    }

    private static EmployeesController CreateController(IEmployeeService service)
    {
        return new EmployeesController(
            service,
            new CreateEmployeeRequestValidator(),
            new UpdateEmployeeRequestValidator());
    }

    private static EmployeeDto CreateEmployeeDto()
    {
        return new EmployeeDto(
            Guid.NewGuid(),
            "EMP-1001",
            "Alex",
            "Morgan",
            "alex.morgan@example.com",
            "Engineer",
            Guid.NewGuid(),
            "Engineering",
            new DateOnly(2024, 1, 15),
            true);
    }
}
