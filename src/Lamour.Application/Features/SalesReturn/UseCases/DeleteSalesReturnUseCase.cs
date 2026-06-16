using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class DeleteSalesReturnUseCase : IDeleteSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<DeleteSalesReturnUseCase> _logger;

    public DeleteSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IUnitOfWork uow,
        ILogger<DeleteSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        await _uow.BeginAsync(ct);
        try
        {
            // Undo the return: stock decreases back
            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity -= line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
            }

            await _repo.DeleteAsync(salesReturn, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Deleted SalesReturn {Id} ({DocumentNumber})", id, salesReturn.DocumentNumber);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
