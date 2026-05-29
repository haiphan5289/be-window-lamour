using Lamour.Application.Features.Sales.Repositories;
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

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var nextNum = await _repo.GetNextCodeNumberAsync(ct);
        var code    = $"BC{nextNum:D5}";
        _logger.LogInformation("Next sales order code: {Code}", code);
        return code;
    }
}
