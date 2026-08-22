using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetNextReceiptCodeUseCase : IGetNextReceiptCodeUseCase
{
    private const string Prefix = "PT";

    private readonly IReceiptRepository _repo;
    private readonly ILogger<GetNextReceiptCodeUseCase> _logger;

    public GetNextReceiptCodeUseCase(IReceiptRepository repo, ILogger<GetNextReceiptCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var nextNum = await _repo.GetNextCodeNumberAsync(Prefix, ct);
        var code    = $"{Prefix}{nextNum:D5}";
        _logger.LogInformation("Next receipt code: {Code}", code);
        return code;
    }
}
