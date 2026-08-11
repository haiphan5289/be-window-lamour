using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Departments.Repositories;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int departmentId, CancellationToken ct = default);
    Task<Department> AddAsync(Department department, CancellationToken ct = default);
    Task<Department> UpdateAsync(Department department, CancellationToken ct = default);
    Task DeleteAsync(Department department, CancellationToken ct = default);
}
