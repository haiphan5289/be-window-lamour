using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class DeleteSalesOrderUseCase : IDeleteSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly ILogger<DeleteSalesOrderUseCase> _logger;

    public DeleteSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        ILogger<DeleteSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        // Restore stock for non-promotion lines
        foreach (var line in order.Lines.Where(l => !l.IsPromotion))
        {
            var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
            if (product is not null)
            {
                product.StockQuantity += line.Quantity;
                await _productRepo.UpdateAsync(product, ct);
            }
        }

        await _repo.DeleteAsync(order, ct);

        _logger.LogInformation("Deleted SalesOrder {Id} ({DocumentNumber})", id, order.DocumentNumber);
    }
}
