using Lamour.Application.Features.Products.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class GetNextProductCodeUseCase : IGetNextProductCodeUseCase
{
    private readonly IProductRepository _repo;
    private readonly ILogger<GetNextProductCodeUseCase> _logger;

    public GetNextProductCodeUseCase(IProductRepository repo, ILogger<GetNextProductCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var code = await _repo.GetNextCodeAsync(ct);
        _logger.LogInformation("Next product code: {Code}", code);
        return code;
    }
}
