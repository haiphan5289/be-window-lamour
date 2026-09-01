using Lamour.Application.Abstractions;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class DeleteSalesReturnUseCase : IDeleteSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<DeleteSalesReturnUseCase> _logger;

    public DeleteSalesReturnUseCase(
        ISalesReturnRepository repo,
        IUnitOfWork uow,
        ILogger<DeleteSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        if (salesReturn.Status != SalesReturnStatus.Draft)
            throw new DomainException("Chỉ chứng từ ở trạng thái Nháp mới được xóa. Bỏ ghi trước khi xóa.");

        await _uow.BeginAsync(ct);
        try
        {
            // Chứng từ còn Draft chưa từng tác động tồn kho (chỉ Confirm mới cộng kho), nên xóa
            // không cần hoàn tác tồn kho gì.
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
