using Lamour.Application.Features.SalesReturn.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class GetNextSalesReturnCodeUseCase : IGetNextSalesReturnCodeUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly ILogger<GetNextSalesReturnCodeUseCase> _logger;

    public GetNextSalesReturnCodeUseCase(ISalesReturnRepository repo, ILogger<GetNextSalesReturnCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var nextNum = await _repo.GetNextCodeNumberAsync(ct);
        var code    = $"BTL{nextNum:D5}";
        _logger.LogInformation("Next sales return code: {Code}", code);
        return code;
    }
}
