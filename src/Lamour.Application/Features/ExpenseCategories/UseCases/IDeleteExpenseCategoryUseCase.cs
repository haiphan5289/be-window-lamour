namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public interface IDeleteExpenseCategoryUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
