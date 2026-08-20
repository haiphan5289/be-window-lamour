using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class GetNextSalesOrderCodeUseCase : IGetNextSalesOrderCodeUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<GetNextSalesOrderCodeUseCase> _logger;

    public GetNextSalesOrderCodeUseCase(ISalesOrderRepository repo, ILogger<GetNextSalesOrderCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(SalesOrderCodeSource source = SalesOrderCodeSource.WarehouseExport, CancellationToken ct = default)
    {
        // BH (Direct) và XK (WarehouseExport) là 2 chuỗi số đếm độc lập theo prefix — mọi business
        // rule khác (trừ tồn kho, VAT, báo cáo...) giữ nguyên như nhau, chỉ khác DocumentNumber.
        var prefix  = source == SalesOrderCodeSource.Direct ? "BH" : "XK";
        var nextNum = await _repo.GetNextCodeNumberAsync(prefix, ct);
        var code    = $"{prefix}{nextNum:D5}";
        _logger.LogInformation("Next sales order code: {Code}", code);
        return code;
    }
}
