using EnterpriseEmployeeManagementAPI.Models.DTOs;

namespace EnterpriseEmployeeManagementAPI.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmployeeDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken);

    Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
