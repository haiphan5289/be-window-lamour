using Lamour.Application.Features.ExpenseCategories.Dtos;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public interface ICreateExpenseCategoryUseCase
{
    Task<ExpenseCategoryResponseDto> ExecuteAsync(CreateExpenseCategoryRequestDto request, CancellationToken ct = default);
}
