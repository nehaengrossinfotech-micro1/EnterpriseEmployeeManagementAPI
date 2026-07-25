using EnterpriseEmployeeManagementAPI.Interfaces;
using EnterpriseEmployeeManagementAPI.Models.DTOs;
using EnterpriseEmployeeManagementAPI.Models.Entities;

namespace EnterpriseEmployeeManagementAPI.Services;

public sealed class EmployeeService(
    IEmployeeRepository employeeRepository,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    private static readonly Action<ILogger, Guid, Exception?> EmployeeCreated =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1001, nameof(EmployeeCreated)),
            "Created employee {EmployeeId}");

    private static readonly Action<ILogger, Guid, Exception?> EmployeeUpdated =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1002, nameof(EmployeeUpdated)),
            "Updated employee {EmployeeId}");

    private static readonly Action<ILogger, Guid, Exception?> EmployeeDeleted =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1003, nameof(EmployeeDeleted)),
            "Deleted employee {EmployeeId}");

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(MapToDto).ToArray();
    }

    public async Task<EmployeeDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var employee = await GetRequiredEmployeeAsync(id, cancellationToken);
        return MapToDto(employee);
    }

    public async Task<IReadOnlyList<EmployeeDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync(cancellationToken);
        }

        var employees = await employeeRepository.SearchAsync(query, cancellationToken);
        return employees.Select(MapToDto).ToArray();
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureReferencesAreValidAsync(
            request.Email,
            request.DepartmentId,
            null,
            cancellationToken);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = request.EmployeeNumber.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            JobTitle = request.JobTitle.Trim(),
            DepartmentId = request.DepartmentId,
            HireDate = request.HireDate,
            CreatedAtUtc = DateTime.UtcNow,
        };

        employee = await employeeRepository.AddAsync(employee, cancellationToken);
        EmployeeCreated(logger, employee.Id, null);
        return MapToDto(employee);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await GetRequiredEmployeeAsync(id, cancellationToken);
        await EnsureReferencesAreValidAsync(
            request.Email,
            request.DepartmentId,
            id,
            cancellationToken);

        employee.EmployeeNumber = request.EmployeeNumber.Trim();
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim().ToLowerInvariant();
        employee.JobTitle = request.JobTitle.Trim();
        employee.DepartmentId = request.DepartmentId;
        employee.HireDate = request.HireDate;
        employee.IsActive = request.IsActive;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        await employeeRepository.UpdateAsync(employee, cancellationToken);
        EmployeeUpdated(logger, id, null);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await GetRequiredEmployeeAsync(id, cancellationToken);
        await employeeRepository.DeleteAsync(employee, cancellationToken);
        EmployeeDeleted(logger, id, null);
    }

    private async Task<Employee> GetRequiredEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{id}' was not found.");
    }

    private async Task EnsureReferencesAreValidAsync(
        string email,
        Guid departmentId,
        Guid? excludingEmployeeId,
        CancellationToken cancellationToken)
    {
        if (await employeeRepository.EmailExistsAsync(
                email.Trim().ToLowerInvariant(),
                excludingEmployeeId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"An employee with email '{email}' already exists.");
        }

        if (!await employeeRepository.DepartmentExistsAsync(
                departmentId,
                cancellationToken))
        {
            throw new KeyNotFoundException(
                $"Department '{departmentId}' was not found.");
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto(
            employee.Id,
            employee.EmployeeNumber,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.JobTitle,
            employee.DepartmentId,
            employee.Department.Name,
            employee.HireDate,
            employee.IsActive);
    }
}
