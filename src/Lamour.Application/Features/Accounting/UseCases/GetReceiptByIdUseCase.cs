using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetReceiptByIdUseCase : IGetReceiptByIdUseCase
{
    private readonly IReceiptRepository _repo;
    private readonly ILogger<GetReceiptByIdUseCase> _logger;

    public GetReceiptByIdUseCase(IReceiptRepository repo, ILogger<GetReceiptByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching receipt {Id}", id);
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");
        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
