using Lamour.Application.Features.Customers.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class GetNextCustomerCodeUseCase : IGetNextCustomerCodeUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<GetNextCustomerCodeUseCase> _logger;

    public GetNextCustomerCodeUseCase(ICustomerRepository repo, ILogger<GetNextCustomerCodeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(CancellationToken ct = default)
    {
        var code = await _repo.GetNextCodeAsync(ct);
        _logger.LogInformation("Next customer code: {Code}", code);
        return code;
    }
}
