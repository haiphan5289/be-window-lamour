using Lamour.Application.Features.ExpenseCategories.Dtos;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public interface IGetExpenseCategoriesUseCase
{
    Task<IEnumerable<ExpenseCategoryResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
