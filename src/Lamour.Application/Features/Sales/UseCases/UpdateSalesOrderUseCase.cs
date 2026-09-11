using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class UpdateSalesOrderUseCase : IUpdateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IWarehouseRepository  _warehouseRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<UpdateSalesOrderUseCase> _logger;

    public UpdateSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<UpdateSalesOrderUseCase> logger)
    {
        _repo          = repo;
        _productRepo   = productRepo;
        _warehouseRepo = warehouseRepo;
        _stockRepo     = stockRepo;
        _uow           = uow;
        _logger        = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(
        int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        // "Ít nhất 1 dòng" không còn bắt buộc ở BE (2026-08-25) — xem CreateSalesOrderUseCase.

        // 2026-09-10: "Cất" (Update) LUÔN đưa đơn về Held — chỉ hoàn tác tồn kho dòng cũ nếu đơn
        // ĐANG Normal (đã từng trừ kho thật lúc Confirm trước đó). Đơn đang Held (chưa từng trừ kho)
        // hoặc Draft (Bỏ ghi đã tự hoàn tồn kho rồi) thì KHÔNG hoàn tác lần nữa (double-revert).
        var stockNotCurrentlyDeducted = order.Status is SalesOrderStatus.Held or SalesOrderStatus.Draft;

        await _uow.BeginAsync(ct);
        try
        {
            // Restore stock from old lines — chỉ khi đơn cũ đã Normal (đã từng trừ kho thật).
            if (!stockNotCurrentlyDeducted)
            {
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
                    await _stockRepo.AdjustQuantityAsync(oldLine.ProductId, oldLine.WarehouseId!.Value, oldLine.Quantity, ct);
                }
            }

            // Build new lines — validate stock against restored quantities
            var stockErrors = new List<string>();
            var newLines = new List<SalesOrderLine>();
            foreach (var dto in request.Lines)
            {
                var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
                if (product is null)
                    throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
                if (!dto.IsPromotion && !product.IsDepositProduct)
                {
                    // Validate Kho tồn tại — cùng lý do CreateSalesOrderUseCase (tránh FK vi phạm
                    // rơi xuống DbUpdateException 500 chung chung).
                    var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId, ct);
                    if (warehouse is null)
                        throw new DomainException($"Vui lòng chọn Kho cho hàng hóa '{product.Name}'.");

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
                    WarehouseId       = product.IsDepositProduct ? null : dto.WarehouseId,
                    ProductCode       = product.Code,
                    ProductName       = product.Name,
                    IsPromotion       = dto.IsPromotion,
                    IsDepositProduct  = product.IsDepositProduct,
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
            }

            if (stockErrors.Count > 0)
                throw new DomainException("Các sản phẩm không đủ tồn kho:\n" + string.Join("\n", stockErrors));

            order.DocumentNumber = request.DocumentNumber;
            order.AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
            order.DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
            order.CustomerId     = request.CustomerId;
            order.CustomerNameOverride = string.IsNullOrWhiteSpace(request.CustomerNameOverride) ? null : request.CustomerNameOverride.Trim();
            order.CustomerAddressOverride = string.IsNullOrWhiteSpace(request.CustomerAddressOverride) ? null : request.CustomerAddressOverride.Trim();
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
            // 2026-09-10: "Cất" (Update) không còn tự Ghi sổ — luôn đưa đơn về Held ("Treo"), bất kể
            // trạng thái trước đó (Held/Draft/Normal). Trừ kho thật xảy ra riêng ở
            // ConfirmSalesOrderUseCase ("Ghi sổ") — xem block hoàn tác dòng cũ ở trên, giữ nguyên vì
            // vẫn cần hoàn tác nếu đơn cũ đang Normal trước khi sửa.
            order.Status         = SalesOrderStatus.Held;
            order.TotalAmount    = newLines.Sum(l => l.Amount);
            order.TotalTaxAmount = newLines.Sum(l => l.TaxAmount);
            order.GrandTotal     = newLines.Sum(l => l.Amount + l.TaxAmount);
            order.Lines          = newLines;

            await _repo.UpdateAsync(order, ct);

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
