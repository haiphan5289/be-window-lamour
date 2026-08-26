using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

using SalesReturnLineEntity = Lamour.Domain.Entities.SalesReturnLine;
using SalesReturnTypeEnum   = Lamour.Domain.Entities.SalesReturnType;

public class UpdateSalesReturnUseCase : IUpdateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<UpdateSalesReturnUseCase> _logger;

    public UpdateSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<UpdateSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesReturnResponseDto> ExecuteAsync(
        int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        await _uow.BeginAsync(ct);
        try
        {
            // Undo old lines: stock goes back down (undo the return)
            foreach (var oldLine in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(oldLine.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity -= oldLine.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(oldLine.ProductId, oldLine.WarehouseId, -oldLine.Quantity, ct);
            }

            // Build new lines
            var newLines = new List<SalesReturnLineEntity>();
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

                var taxRate    = SalesOrderTaxCalculator.ToPercent(product.VatRate);
                var taxAmount  = (amount - discountAmount) * taxRate / 100m;
                var costPrice  = product.CostPrice;
                var costAmount = dto.Quantity * costPrice;

                newLines.Add(new SalesReturnLineEntity
                {
                    ProductId        = dto.ProductId,
                    WarehouseId      = dto.WarehouseId,
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
                    TaxRate          = taxRate,
                    TaxAmount        = taxAmount,
                    TaxAccount       = string.IsNullOrWhiteSpace(dto.TaxAccount) ? "33311" : dto.TaxAccount,
                    CostAccount      = string.IsNullOrWhiteSpace(dto.CostAccount) ? "1561" : dto.CostAccount,
                    CogsAccount      = string.IsNullOrWhiteSpace(dto.CogsAccount) ? "632"  : dto.CogsAccount,
                    CostPrice        = costPrice,
                    CostAmount       = costAmount,
                    DepartmentId     = dto.DepartmentId,
                });
            }

            salesReturn.DocumentNumber = request.DocumentNumber;
            salesReturn.AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
            salesReturn.DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
            salesReturn.CustomerId     = request.CustomerId;
            salesReturn.EmployeeId     = request.EmployeeId;
            salesReturn.Description    = request.Description;
            salesReturn.Reference      = request.Reference;
            salesReturn.ReturnType     = (SalesReturnTypeEnum)request.ReturnType;
            salesReturn.TotalAmount    = newLines.Sum(l => l.Amount);
            salesReturn.TotalDiscount  = newLines.Sum(l => l.DiscountAmount);
            salesReturn.TotalPayment   = newLines.Sum(l => l.Amount) - newLines.Sum(l => l.DiscountAmount);
            salesReturn.Lines          = newLines;

            await _repo.UpdateAsync(salesReturn, ct);

            // Apply new lines: restore stock for new returned items
            foreach (var line in newLines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity += line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, line.Quantity, ct);
            }

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Updated SalesReturn {Id}", id);

            var updated = await _repo.GetByIdAsync(id, ct);
            return GetSalesReturnsUseCase.MapToDto(updated!);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
