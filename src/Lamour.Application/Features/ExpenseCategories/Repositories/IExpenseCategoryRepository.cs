using Lamour.Domain.Entities;

namespace Lamour.Application.Features.ExpenseCategories.Repositories;

public interface IExpenseCategoryRepository
{
    Task<IEnumerable<ExpenseCategory>> GetAllAsync(CancellationToken ct = default);
    Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<ExpenseCategory> AddAsync(ExpenseCategory category, CancellationToken ct = default);
    Task<ExpenseCategory> UpdateAsync(ExpenseCategory category, CancellationToken ct = default);
    Task DeleteAsync(ExpenseCategory category, CancellationToken ct = default);
}
