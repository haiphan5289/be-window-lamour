using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

using SalesReturnEntity     = Lamour.Domain.Entities.SalesReturn;
using SalesReturnLineEntity = Lamour.Domain.Entities.SalesReturnLine;
using SalesReturnTypeEnum   = Lamour.Domain.Entities.SalesReturnType;
using SalesReturnStatusEnum = Lamour.Domain.Entities.SalesReturnStatus;

public class CreateSalesReturnUseCase : ICreateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IWarehouseRepository   _warehouseRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<CreateSalesReturnUseCase> _logger;

    public CreateSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<CreateSalesReturnUseCase> logger)
    {
        _repo          = repo;
        _productRepo   = productRepo;
        _warehouseRepo = warehouseRepo;
        _stockRepo     = stockRepo;
        _uow           = uow;
        _logger        = logger;
    }

    public async Task<SalesReturnResponseDto> ExecuteAsync(
        CreateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        var lines = new List<SalesReturnLineEntity>();
        foreach (var dto in request.Lines)
        {
            // Tracked (không phải AsNoTracking) — 2026-09-11 Cất giờ cộng tồn kho ngay trong cùng
            // vòng lặp validate (xem cuối khối này), cần entity tracked để EF ghi nhận thay đổi
            // StockQuantity, khớp đúng cách ConfirmSalesReturnUseCase (cũ) đã làm.
            var product = await _productRepo.GetByIdTrackedAsync(dto.ProductId, ct);
            if (product is null)
                throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
            if (!product.IsActive)
                throw new DomainException($"Hàng hóa '{product.Name}' đã ngưng kinh doanh.");

            // Validate Kho tồn tại — thiếu bước này khiến "warehouse_id" (FK thật tới bảng
            // warehouses) nhận giá trị 0/không hợp lệ từ client (chưa chọn Kho) rơi thẳng xuống
            // DbUpdateException "FK_sales_return_lines_warehouses_warehouse_id" (500 chung chung,
            // không rõ nguyên nhân) thay vì 1 lỗi 400 dễ hiểu.
            var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId, ct);
            if (warehouse is null)
                throw new DomainException($"Vui lòng chọn Kho cho hàng hóa '{product.Name}'.");

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

            // 2026-09-11: Cất giờ CỘNG tồn kho ngay (hàng trả lại = nhập lại kho) — đảo ngược quyết
            // định 2026-09-10 ("Cất luôn ra Held, không đụng kho, phải Ghi sổ riêng"). Theo yêu cầu
            // khớp hành vi MISA: "Cất" = "Ghi sổ" luôn, không còn bước Treo trung gian nữa. Logic
            // cộng kho giống hệt ConfirmSalesReturnUseCase (vẫn giữ, dùng khi bấm "Ghi sổ" thẳng từ
            // trạng thái Nháp không qua Sửa/Cất).
            product.StockQuantity += dto.Quantity;
            await _productRepo.UpdateAsync(product, ct);
            await _stockRepo.AdjustQuantityAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, ct);
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
            // 2026-09-11: "Cất" = "Ghi sổ" ngay — chứng từ mới luôn ở Confirmed, cộng tồn kho ngay
            // trong vòng lặp validate ở trên (đảo ngược quyết định 2026-09-10 "Cất luôn ra Held").
            Status         = SalesReturnStatusEnum.Confirmed,
            ConfirmedAt    = DateTime.UtcNow,
            TotalAmount    = lines.Sum(l => l.Amount),
            TotalDiscount  = lines.Sum(l => l.DiscountAmount),
            TotalPayment   = lines.Sum(l => l.Amount) - lines.Sum(l => l.DiscountAmount),
            TotalTaxAmount = lines.Sum(l => l.TaxAmount),
            GrandTotal     = lines.Sum(l => l.Amount) - lines.Sum(l => l.DiscountAmount) + lines.Sum(l => l.TaxAmount),
            CreatedAt      = DateTime.UtcNow,
            Lines          = lines,
        };

        await _uow.BeginAsync(ct);
        try
        {
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
