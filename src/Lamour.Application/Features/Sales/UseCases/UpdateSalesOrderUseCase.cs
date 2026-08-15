using Lamour.Application.Abstractions;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class UpdateSalesOrderUseCase : IUpdateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IDepositRepository    _depositRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<UpdateSalesOrderUseCase> _logger;

    public UpdateSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IDepositRepository depositRepo,
        IUnitOfWork uow,
        ILogger<UpdateSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _depositRepo = depositRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(
        int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        await _uow.BeginAsync(ct);
        try
        {
            // Restore stock from old lines
            foreach (var oldLine in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(oldLine.ProductId, ct);
                if (product is not null && product.IsDepositProduct)
                    continue; // "Đặt cọc" không phải hàng tồn kho thật

                if (product is not null)
                {
                    product.StockQuantity += oldLine.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(oldLine.ProductId, oldLine.WarehouseId, oldLine.Quantity, ct);
            }

            // Build new lines — validate stock against restored quantities
            var stockErrors = new List<string>();
            var newLines = new List<SalesOrderLine>();
            decimal depositLinesAmount = 0;
            foreach (var dto in request.Lines)
            {
                var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
                if (product is null)
                    throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
                if (!dto.IsPromotion && !product.IsDepositProduct)
                {
                    var availableQty = await _stockRepo.GetQuantityAsync(dto.ProductId, dto.WarehouseId, ct);
                    if (availableQty < dto.Quantity)
                        stockErrors.Add($"• {product.Name}: kho có {availableQty}, cần {dto.Quantity}");
                }

                // Hàng khuyến mại: giá/CK/thuế luôn = 0, bất kể client gửi gì lên.
                var unitPrice      = dto.IsPromotion ? 0m : dto.UnitPrice;
                var discountRate   = dto.IsPromotion ? 0m : Math.Max(0, Math.Min(100, dto.DiscountRate));
                var isAmountManual = !dto.IsPromotion && dto.IsAmountManual;
                if (isAmountManual && dto.Amount < 0)
                    throw new DomainException($"Thành tiền dòng '{product.Name}' không được âm.");
                var amount = dto.IsPromotion
                    ? 0m
                    : isAmountManual ? dto.Amount : dto.Quantity * unitPrice * (1 - discountRate / 100m);
                var taxRate      = dto.IsPromotion ? 0m : SalesOrderTaxCalculator.ToPercent(product.VatRate);
                newLines.Add(new SalesOrderLine
                {
                    ProductId         = dto.ProductId,
                    WarehouseId       = dto.WarehouseId,
                    ProductCode       = product.Code,
                    ProductName       = product.Name,
                    IsPromotion       = dto.IsPromotion,
                    Unit              = string.IsNullOrWhiteSpace(dto.Unit) ? product.Unit : dto.Unit,
                    Quantity          = dto.Quantity,
                    UnitPrice         = unitPrice,
                    DiscountRate      = discountRate,
                    Amount            = amount,
                    IsAmountManual    = isAmountManual,
                    TaxRate           = taxRate,
                    TaxAmount         = amount * taxRate / 100m,
                    ReceivableAccount = string.IsNullOrWhiteSpace(dto.ReceivableAccount) ? "131" : dto.ReceivableAccount,
                    RevenueAccount    = string.IsNullOrWhiteSpace(dto.RevenueAccount) ? "511" : dto.RevenueAccount,
                });

                if (product.IsDepositProduct)
                    depositLinesAmount += amount;
            }

            if (stockErrors.Count > 0)
                throw new DomainException("Các sản phẩm không đủ tồn kho:\n" + string.Join("\n", stockErrors));

            order.DocumentNumber = request.DocumentNumber;
            order.AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
            order.DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
            order.CustomerId     = request.CustomerId;
            order.EmployeeId     = request.EmployeeId;
            order.Description    = request.Description;
            order.Reference      = request.Reference;
            order.PaymentTerms   = request.PaymentTerms;
            order.PaymentDueDays = request.PaymentDueDays;
            order.PaymentDueDate = request.PaymentDueDate.HasValue
                ? DateTime.SpecifyKind(request.PaymentDueDate.Value, DateTimeKind.Utc)
                : null;
            order.Notes          = request.Notes;
            order.DeliveryMethod = request.DeliveryMethod;
            order.PaymentMethod  = request.PaymentMethod;
            // "💾 Ghi sổ" luôn post lại đơn hàng — sửa 1 đơn đang Treo rồi Ghi sổ phải đưa
            // về Normal (khớp hành vi CreateSalesOrderUseCase); chỉ nút "⏸ Treo" riêng mới giữ Treo.
            order.Status         = SalesOrderStatus.Normal;
            order.TotalAmount    = newLines.Sum(l => l.Amount);
            order.TotalTaxAmount = newLines.Sum(l => l.TaxAmount);
            order.GrandTotal     = newLines.Sum(l => l.Amount + l.TaxAmount);
            order.Lines          = newLines;

            await _repo.UpdateAsync(order, ct);

            // Apply stock for new non-promotion lines
            foreach (var line in newLines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null && product.IsDepositProduct)
                    continue; // "Đặt cọc" không phải hàng tồn kho thật

                if (product is not null)
                {
                    product.StockQuantity -= line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, -line.Quantity, ct);
            }

            await SalesOrderDepositHelper.SyncAsync(_depositRepo, order, depositLinesAmount, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Updated SalesOrder {Id}", id);

            var updated = await _repo.GetByIdAsync(id, ct);
            return GetSalesOrdersUseCase.MapToDto(updated!);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
