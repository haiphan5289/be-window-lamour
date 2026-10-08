using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class GetNextWarehouseReceiptNumberUseCase : IGetNextWarehouseReceiptNumberUseCase
{
    private readonly IWarehouseReceiptRepository _repo;
    private readonly ILogger<GetNextWarehouseReceiptNumberUseCase> _logger;

    public GetNextWarehouseReceiptNumberUseCase(
        IWarehouseReceiptRepository repo, ILogger<GetNextWarehouseReceiptNumberUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var code = await _repo.GetNextReceiptNumberAsync(ct);
        _logger.LogInformation("Next warehouse receipt number: {Code}", code);
        return code;
    }
}
