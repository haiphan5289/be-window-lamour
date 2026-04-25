using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class DeleteProductUseCase : IDeleteProductUseCase
{
    private readonly IProductRepository         _repo;
    private readonly ILogger<DeleteProductUseCase> _logger;

    public DeleteProductUseCase(IProductRepository repo, ILogger<DeleteProductUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        await _repo.DeleteAsync(product, ct);
        _logger.LogInformation("Deleted product {Id}", id);
    }
}
