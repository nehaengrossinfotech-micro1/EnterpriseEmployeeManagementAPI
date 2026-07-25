namespace EnterpriseEmployeeManagementAPI.Models.Entities;

public sealed class Department
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Employee> Employees { get; } = [];
}
