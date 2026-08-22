using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetOutstandingSalesOrdersUseCase
{
    Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default);
}
