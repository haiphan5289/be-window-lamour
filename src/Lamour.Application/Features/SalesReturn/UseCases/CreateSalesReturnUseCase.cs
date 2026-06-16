using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

using SalesReturnEntity     = Lamour.Domain.Entities.SalesReturn;
using SalesReturnLineEntity = Lamour.Domain.Entities.SalesReturnLine;
using SalesReturnTypeEnum   = Lamour.Domain.Entities.SalesReturnType;

public class CreateSalesReturnUseCase : ICreateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<CreateSalesReturnUseCase> _logger;

    public CreateSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IUnitOfWork uow,
        ILogger<CreateSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesReturnResponseDto> ExecuteAsync(
        CreateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        var lines = new List<SalesReturnLineEntity>();
        foreach (var dto in request.Lines)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
            if (product is null)
                throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
            if (!product.IsActive)
                throw new DomainException($"Hàng hóa '{product.Name}' đã ngưng kinh doanh.");

            var discountRate   = Math.Max(0, Math.Min(100, dto.DiscountRate));
            var amount         = dto.Quantity * dto.UnitPrice;
            var discountAmount = amount * discountRate / 100m;

            lines.Add(new SalesReturnLineEntity
            {
                ProductId        = dto.ProductId,
                ProductCode      = product.Code,
                ProductName      = product.Name,
                ReturnAccount    = string.IsNullOrWhiteSpace(dto.ReturnAccount)   ? "5212" : dto.ReturnAccount,
                DebtAccount      = string.IsNullOrWhiteSpace(dto.DebtAccount)     ? "131"  : dto.DebtAccount,
                DiscountAccount  = string.IsNullOrWhiteSpace(dto.DiscountAccount) ? "5211" : dto.DiscountAccount,
                Unit             = string.IsNullOrWhiteSpace(dto.Unit)            ? product.Unit : dto.Unit,
                Quantity         = dto.Quantity,
                UnitPrice        = dto.UnitPrice,
                Amount           = amount,
                DiscountRate     = discountRate,
                DiscountAmount   = discountAmount,
                SalesOrderNumber = dto.SalesOrderNumber,
            });
        }

        var salesReturn = new SalesReturnEntity
        {
            DocumentNumber = request.DocumentNumber,
            AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc),
            CustomerId     = request.CustomerId,
            EmployeeId     = request.EmployeeId,
            Description    = request.Description,
            Reference      = request.Reference,
            ReturnType     = (SalesReturnTypeEnum)request.ReturnType,
            TotalAmount    = lines.Sum(l => l.Amount),
            TotalDiscount  = lines.Sum(l => l.DiscountAmount),
            TotalPayment   = lines.Sum(l => l.Amount) - lines.Sum(l => l.DiscountAmount),
            CreatedAt      = DateTime.UtcNow,
            Lines          = lines,
        };

        await _uow.BeginAsync(ct);
        try
        {
            var saved = await _repo.AddAsync(salesReturn, ct);

            // Restore stock — items returned means stock increases
            foreach (var line in lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity += line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
            }

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Created SalesReturn {DocumentNumber} for customer {CustomerId}",
                saved.DocumentNumber, saved.CustomerId);

            return GetSalesReturnsUseCase.MapToDto(saved);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
