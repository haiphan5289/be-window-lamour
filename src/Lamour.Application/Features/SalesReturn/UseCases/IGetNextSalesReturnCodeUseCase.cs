namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IGetNextSalesReturnCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
