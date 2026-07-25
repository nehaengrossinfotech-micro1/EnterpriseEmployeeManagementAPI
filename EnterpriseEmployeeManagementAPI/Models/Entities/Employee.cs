namespace EnterpriseEmployeeManagementAPI.Models.Entities;

public sealed class Employee
{
    public Guid Id { get; set; }

    public required string EmployeeNumber { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required string JobTitle { get; set; }

    public Guid DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
