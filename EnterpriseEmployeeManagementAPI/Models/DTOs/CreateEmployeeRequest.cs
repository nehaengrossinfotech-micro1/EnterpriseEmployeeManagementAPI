namespace EnterpriseEmployeeManagementAPI.Models.DTOs;

public sealed record CreateEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    Guid DepartmentId,
    DateOnly HireDate);
