using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class GetSalesReturnByIdUseCase : IGetSalesReturnByIdUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly ILogger<GetSalesReturnByIdUseCase> _logger;

    public GetSalesReturnByIdUseCase(ISalesReturnRepository repo, ILogger<GetSalesReturnByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<SalesReturnResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales return {Id}", id);
        var salesReturn = await _repo.GetByIdAsync(id, ct);
        return salesReturn is null ? null : GetSalesReturnsUseCase.MapToDto(salesReturn);
    }
}
