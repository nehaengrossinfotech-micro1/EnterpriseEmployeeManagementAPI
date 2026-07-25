using EnterpriseEmployeeManagementAPI.Models.Entities;

namespace EnterpriseEmployeeManagementAPI.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken);

    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Employee>> SearchAsync(
        string query,
        CancellationToken cancellationToken);

    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken);

    Task UpdateAsync(Employee employee, CancellationToken cancellationToken);

    Task DeleteAsync(Employee employee, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string email,
        Guid? excludingEmployeeId,
        CancellationToken cancellationToken);

    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
}
