using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.Interfaces;
using EnterpriseEmployeeManagementAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementAPI.Repositories;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.Department)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Employees
            .Include(employee => employee.Department)
            .SingleOrDefaultAsync(employee => employee.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var pattern = $"%{query.Trim()}%";

        return await dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.Department)
            .Where(employee =>
                EF.Functions.Like(employee.EmployeeNumber, pattern)
                || EF.Functions.Like(employee.FirstName, pattern)
                || EF.Functions.Like(employee.LastName, pattern)
                || EF.Functions.Like(employee.Email, pattern)
                || EF.Functions.Like(employee.JobTitle, pattern)
                || EF.Functions.Like(employee.Department.Name, pattern))
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee> AddAsync(
        Employee employee,
        CancellationToken cancellationToken)
    {
        await dbContext.Employees.AddAsync(employee, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(employee).Reference(item => item.Department).LoadAsync(cancellationToken);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken)
    {
        dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken)
    {
        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? excludingEmployeeId,
        CancellationToken cancellationToken)
    {
        return dbContext.Employees.AnyAsync(
            employee =>
                employee.Email == email
                && (!excludingEmployeeId.HasValue || employee.Id != excludingEmployeeId.Value),
            cancellationToken);
    }

    public Task<bool> DepartmentExistsAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        return dbContext.Departments.AnyAsync(
            department => department.Id == departmentId,
            cancellationToken);
    }
}
