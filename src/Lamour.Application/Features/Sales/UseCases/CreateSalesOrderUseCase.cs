using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class CreateSalesOrderUseCase : ICreateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<CreateSalesOrderUseCase> _logger;

    public CreateSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<CreateSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(
        CreateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        // Validate products, stock, and build lines
        var stockErrors = new List<string>();
        var lines = new List<SalesOrderLine>();
        foreach (var dto in request.Lines)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
            if (product is null)
                throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
            if (!product.IsActive)
                throw new DomainException($"Hàng hóa '{product.Name}' đã ngưng kinh doanh.");
            if (!dto.IsPromotion)
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
            lines.Add(new SalesOrderLine
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
        }

        if (stockErrors.Count > 0)
            throw new DomainException("Các sản phẩm không đủ tồn kho:\n" + string.Join("\n", stockErrors));

        var order = new SalesOrder
        {
            DocumentNumber = request.DocumentNumber,
            AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc),
            CustomerId     = request.CustomerId,
            EmployeeId     = request.EmployeeId,
            Description    = request.Description,
            Reference      = request.Reference,
            PaymentTerms   = request.PaymentTerms,
            PaymentDueDays = request.PaymentDueDays,
            PaymentDueDate = request.PaymentDueDate.HasValue
                ? DateTime.SpecifyKind(request.PaymentDueDate.Value, DateTimeKind.Utc)
                : null,
            Notes          = request.Notes,
            DeliveryMethod = request.DeliveryMethod,
            PaymentMethod  = request.PaymentMethod,
            TotalAmount    = lines.Sum(l => l.Amount),
            TotalTaxAmount = lines.Sum(l => l.TaxAmount),
            GrandTotal     = lines.Sum(l => l.Amount + l.TaxAmount),
            CreatedAt      = DateTime.UtcNow,
            Status         = SalesOrderStatus.Normal,
            Lines          = lines,
        };

        await _uow.BeginAsync(ct);
        try
        {
            var saved = await _repo.AddAsync(order, ct);

            foreach (var line in lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity -= line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, -line.Quantity, ct);
            }

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Created SalesOrder {DocumentNumber} for customer {CustomerId}",
                saved.DocumentNumber, saved.CustomerId);

            return GetSalesOrdersUseCase.MapToDto(saved);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
