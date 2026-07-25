namespace EnterpriseEmployeeManagementAPI.Models.DTOs;

public sealed record UpdateEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    Guid DepartmentId,
    DateOnly HireDate,
    bool IsActive);
