namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetNextSalesOrderCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
