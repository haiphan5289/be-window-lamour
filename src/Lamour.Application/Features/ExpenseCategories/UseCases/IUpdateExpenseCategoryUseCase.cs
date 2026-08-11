using Lamour.Application.Features.ExpenseCategories.Dtos;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public interface IUpdateExpenseCategoryUseCase
{
    Task<ExpenseCategoryResponseDto> ExecuteAsync(int id, UpdateExpenseCategoryRequestDto request, CancellationToken ct = default);
}
