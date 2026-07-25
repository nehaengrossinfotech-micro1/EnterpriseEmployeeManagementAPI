namespace EnterpriseEmployeeManagementAPI.Models.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    Guid DepartmentId,
    string DepartmentName,
    DateOnly HireDate,
    bool IsActive);
