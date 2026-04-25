using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Employees.Repositories;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Employee?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken ct = default);
    Task<Employee> UpdateAsync(Employee employee, CancellationToken ct = default);
    Task DeleteAsync(Employee employee, CancellationToken ct = default);
}
