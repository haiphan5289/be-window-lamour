namespace Lamour.Application.Features.Categories.UseCases;

public interface IDeleteCategoryUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
