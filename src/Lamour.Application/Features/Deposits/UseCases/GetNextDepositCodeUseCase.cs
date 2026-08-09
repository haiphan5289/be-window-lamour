using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetNextDepositCodeUseCase : IGetNextDepositCodeUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<GetNextDepositCodeUseCase> _logger;

    public GetNextDepositCodeUseCase(IDepositRepository repo, ILogger<GetNextDepositCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var nextNum = await _repo.GetNextCodeNumberAsync(ct);
        var code    = $"DC{nextNum:D5}";
        _logger.LogInformation("Next deposit code: {Code}", code);
        return code;
    }
}
