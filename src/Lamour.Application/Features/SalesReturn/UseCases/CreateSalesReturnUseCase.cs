using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
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
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<CreateSalesReturnUseCase> _logger;

    public CreateSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<CreateSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
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

            // Thuế + giá vốn: BE luôn tự tính từ Product tại thời điểm ghi sổ, bỏ qua tax_rate/
            // cost_price client gửi lên — giống hệt cách SalesOrder xử lý TaxRate/TaxAmount.
            var taxRate    = SalesOrderTaxCalculator.ToPercent(product.VatRate);
            var taxAmount  = (amount - discountAmount) * taxRate / 100m;
            var costPrice  = product.CostPrice;
            var costAmount = dto.Quantity * costPrice;

            lines.Add(new SalesReturnLineEntity
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
            // Draft/Confirmed workflow: stock effect is applied only on Confirm ("Ghi sổ"), not
            // here on Create. New records start as Draft (see SalesReturn.Status property default).
            var saved = await _repo.AddAsync(salesReturn, ct);

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
