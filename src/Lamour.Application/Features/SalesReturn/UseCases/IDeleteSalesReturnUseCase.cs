namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IDeleteSalesReturnUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
