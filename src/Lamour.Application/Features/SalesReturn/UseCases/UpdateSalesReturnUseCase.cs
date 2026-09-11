using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

using SalesReturnLineEntity = Lamour.Domain.Entities.SalesReturnLine;
using SalesReturnTypeEnum   = Lamour.Domain.Entities.SalesReturnType;

public class UpdateSalesReturnUseCase : IUpdateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IWarehouseRepository   _warehouseRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<UpdateSalesReturnUseCase> _logger;

    public UpdateSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<UpdateSalesReturnUseCase> logger)
    {
        _repo          = repo;
        _productRepo   = productRepo;
        _warehouseRepo = warehouseRepo;
        _stockRepo     = stockRepo;
        _uow           = uow;
        _logger        = logger;
    }

    public async Task<SalesReturnResponseDto> ExecuteAsync(
        int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        // "Bỏ ghi" (UnconfirmSalesReturnUseCase) đã tự rút tồn kho của các dòng CŨ khi chuyển về
        // Draft — nếu chứng từ đang Draft lúc vào đây, KHÔNG được rút lại lần nữa (double-revert).
        // Chỉ rút lại dòng cũ khi nó đang Confirmed (dòng cũ đó thực sự vẫn còn cộng trong tồn kho).
        var wasConfirmed = salesReturn.Status == SalesReturnStatus.Confirmed;

        await _uow.BeginAsync(ct);
        try
        {
            // Không còn vòng đời Nháp thường trực: chứng từ Confirmed thì các dòng cũ ĐÃ cộng tồn
            // kho lúc lưu/ghi sổ trước đó — phải rút lại toàn bộ rồi cộng lại theo dòng mới (mirror
            // UpdateSalesOrderUseCase). Hàng trả lại = nhập kho, nên "hoàn tác" ở đây là TRỪ tồn
            // kho → cần kiểm tra đủ tồn trước, tránh rơi về âm nếu đã xuất bán tiếp.
            var oldLines = salesReturn.Lines.ToList();

            if (wasConfirmed)
            {
                foreach (var oldLine in oldLines)
                {
                    var product = await _productRepo.GetByIdTrackedAsync(oldLine.ProductId, ct)
                        ?? throw new DomainException($"Product with id {oldLine.ProductId} not found.");
                    if (product.StockQuantity < oldLine.Quantity)
                        throw new DomainException(
                            $"Không thể sửa vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ để " +
                            "hoàn tác số lượng đã nhập lại kho ở lần lưu trước (đã phát sinh giao dịch xuất kho sau đó).");
                }

                foreach (var oldLine in oldLines)
                {
                    var product = await _productRepo.GetByIdTrackedAsync(oldLine.ProductId, ct);
                    if (product is not null)
                    {
                        product.StockQuantity -= oldLine.Quantity;
                        await _productRepo.UpdateAsync(product, ct);
                    }
                    await _stockRepo.AdjustQuantityAsync(oldLine.ProductId, oldLine.WarehouseId, -oldLine.Quantity, ct);
                }
            }

            var newLines = new List<SalesReturnLineEntity>();
            foreach (var dto in request.Lines)
            {
                // Tracked — 2026-09-11 Cất (Update) giờ cộng tồn kho ngay cho dòng mới (xem cuối
                // khối này), cần entity tracked để EF ghi nhận thay đổi StockQuantity.
                var product = await _productRepo.GetByIdTrackedAsync(dto.ProductId, ct);
                if (product is null)
                    throw new DomainException($"Sản phẩm với id {dto.ProductId} không tồn tại.");
                if (!product.IsActive)
                    throw new DomainException($"Hàng hóa '{product.Name}' đã ngưng kinh doanh.");

                // Validate Kho tồn tại — thiếu bước này khiến "warehouse_id" (FK thật tới bảng
                // warehouses) nhận giá trị 0/không hợp lệ từ client (chưa chọn Kho) rơi thẳng xuống
                // DbUpdateException "FK_sales_return_lines_warehouses_warehouse_id" (500 chung
                // chung, không rõ nguyên nhân) thay vì 1 lỗi 400 dễ hiểu.
                var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId, ct);
                if (warehouse is null)
                    throw new DomainException($"Vui lòng chọn Kho cho hàng hóa '{product.Name}'.");

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

                // 2026-09-11: cộng tồn kho ngay cho dòng mới — "Cất" giờ luôn kết thúc ở Confirmed
                // (xem thay đổi Status bên dưới), mirror CreateSalesReturnUseCase cùng ngày.
                product.StockQuantity += dto.Quantity;
                await _productRepo.UpdateAsync(product, ct);
                await _stockRepo.AdjustQuantityAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, ct);
            }

            salesReturn.DocumentNumber = request.DocumentNumber;
            salesReturn.AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
            salesReturn.DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
            salesReturn.CustomerId     = request.CustomerId;
            salesReturn.EmployeeId     = request.EmployeeId;
            salesReturn.Description    = request.Description;
            salesReturn.Reference      = request.Reference;
            salesReturn.ReturnType     = (SalesReturnTypeEnum)request.ReturnType;
            // 2026-09-11: "Cất" = "Ghi sổ" ngay — đảo ngược quyết định 2026-09-10 ("Cất luôn ra
            // Held"). Luôn đưa chứng từ về Confirmed, cộng tồn kho dòng mới ngay ở vòng lặp trên.
            salesReturn.Status         = SalesReturnStatus.Confirmed;
            salesReturn.ConfirmedAt    = DateTime.UtcNow;
            salesReturn.TotalAmount    = newLines.Sum(l => l.Amount);
            salesReturn.TotalDiscount  = newLines.Sum(l => l.DiscountAmount);
            salesReturn.TotalPayment   = newLines.Sum(l => l.Amount) - newLines.Sum(l => l.DiscountAmount);

            salesReturn.Lines.Clear();
            foreach (var newLine in newLines)
                salesReturn.Lines.Add(newLine);

            await _repo.UpdateAsync(salesReturn, ct);

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
