using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class UpdateSalesOrderUseCase : IUpdateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly ILogger<UpdateSalesOrderUseCase> _logger;

    public UpdateSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        ILogger<UpdateSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _logger      = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(
        int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        // Restore stock from old lines
        foreach (var oldLine in order.Lines.Where(l => !l.IsPromotion))
        {
            var product = await _productRepo.GetByIdTrackedAsync(oldLine.ProductId, ct);
            if (product is not null)
            {
                product.StockQuantity += oldLine.Quantity;
                await _productRepo.UpdateAsync(product, ct);
            }
        }

        // Build new lines
        var newLines = new List<SalesOrderLine>();
        foreach (var dto in request.Lines)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId, ct);
            if (product is null)
                throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");

            var discountRate = Math.Max(0, Math.Min(100, dto.DiscountRate));
            newLines.Add(new SalesOrderLine
            {
                ProductId         = dto.ProductId,
                ProductCode       = product.Code,
                ProductName       = product.Name,
                IsPromotion       = dto.IsPromotion,
                Unit              = string.IsNullOrWhiteSpace(dto.Unit) ? product.Unit : dto.Unit,
                Quantity          = dto.Quantity,
                UnitPrice         = dto.UnitPrice,
                DiscountRate      = discountRate,
                Amount            = dto.Quantity * dto.UnitPrice * (1 - discountRate / 100m),
                ReceivableAccount = string.IsNullOrWhiteSpace(dto.ReceivableAccount) ? "131" : dto.ReceivableAccount,
                RevenueAccount    = string.IsNullOrWhiteSpace(dto.RevenueAccount) ? "511" : dto.RevenueAccount,
            });
        }

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
        order.TotalAmount    = newLines.Sum(l => l.Amount);
        order.Lines          = newLines;

        await _repo.UpdateAsync(order, ct);

        // Apply stock for new non-promotion lines
        foreach (var line in newLines.Where(l => !l.IsPromotion))
        {
            var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
            if (product is not null)
            {
                product.StockQuantity -= line.Quantity;
                await _productRepo.UpdateAsync(product, ct);
            }
        }

        _logger.LogInformation("Updated SalesOrder {Id}", id);

        var updated = await _repo.GetByIdAsync(id, ct);
        return GetSalesOrdersUseCase.MapToDto(updated!);
    }
}
