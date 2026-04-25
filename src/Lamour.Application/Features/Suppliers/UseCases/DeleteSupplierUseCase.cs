using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Suppliers.UseCases;

public class DeleteSupplierUseCase : IDeleteSupplierUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly ILogger<DeleteSupplierUseCase> _logger;

    public DeleteSupplierUseCase(ISupplierRepository repo, ILogger<DeleteSupplierUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var supplier = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Supplier {id} not found.");

        await _repo.DeleteAsync(supplier, ct);
        _logger.LogInformation("Deleted supplier {Id}", id);
    }
}
