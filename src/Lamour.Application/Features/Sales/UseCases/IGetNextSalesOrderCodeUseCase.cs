using Lamour.Domain.Enums;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetNextSalesOrderCodeUseCase
{
    Task<string> ExecuteAsync(SalesOrderCodeSource source = SalesOrderCodeSource.WarehouseExport, CancellationToken ct = default);
}
