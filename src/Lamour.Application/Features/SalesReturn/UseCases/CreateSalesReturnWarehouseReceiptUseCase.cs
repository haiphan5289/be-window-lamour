using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Application.Features.WarehouseReceipts.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

// "Lập PN" — tự động tạo VÀ ghi sổ (Confirmed) 1 WarehouseReceipt (ReceiptType.ReturnedGoods) từ
// 1 SalesReturn đã lưu. KHÔNG dùng ConfirmWarehouseReceiptUseCase (vốn cộng lại Product.StockQuantity)
// vì CreateSalesReturnUseCase/UpdateSalesReturnUseCase đã cộng tồn kho ngay khi ghi sổ chứng từ trả
// hàng — gọi thêm lần nữa sẽ cộng tồn kho 2 lần. PN ở đây thuần là chứng từ giấy/kế toán
// (Nợ TK kho / Có TK giá vốn), dựng sẵn Status = Confirmed để hiện "đã ghi sổ" ngay, không đụng kho.
public class CreateSalesReturnWarehouseReceiptUseCase : ICreateSalesReturnWarehouseReceiptUseCase
{
    private readonly ISalesReturnRepository      _salesReturnRepo;
    private readonly IWarehouseReceiptRepository _receiptRepo;
    private readonly ILogger<CreateSalesReturnWarehouseReceiptUseCase> _logger;

    public CreateSalesReturnWarehouseReceiptUseCase(
        ISalesReturnRepository salesReturnRepo,
        IWarehouseReceiptRepository receiptRepo,
        ILogger<CreateSalesReturnWarehouseReceiptUseCase> logger)
    {
        _salesReturnRepo = salesReturnRepo;
        _receiptRepo     = receiptRepo;
        _logger          = logger;
    }

    public async Task<WarehouseReceiptResponseDto> ExecuteAsync(int salesReturnId, CancellationToken ct = default)
    {
        var salesReturn = await _salesReturnRepo.GetByIdAsync(salesReturnId, ct)
            ?? throw new NotFoundException($"Sales return {salesReturnId} not found.");

        if (salesReturn.Lines.Count == 0)
            throw new DomainException("Chứng từ không có dòng hàng nào để lập phiếu nhập kho.");

        var existingReceipts = await _receiptRepo.GetAllAsync(ct);
        if (existingReceipts.Any(r => r.ReceiptType == WarehouseReceiptType.ReturnedGoods
                                       && r.Reference == salesReturn.DocumentNumber))
            throw new DomainException($"Đã lập phiếu nhập kho cho chứng từ {salesReturn.DocumentNumber} rồi.");

        var receiptNumber = await _receiptRepo.GetNextReceiptNumberAsync(ct);

        var receipt = new WarehouseReceipt
        {
            ReceiptNumber  = receiptNumber,
            ReceiptType    = WarehouseReceiptType.ReturnedGoods,
            Status         = WarehouseReceiptStatus.Confirmed,
            CustomerId     = salesReturn.CustomerId,
            EmployeeId     = salesReturn.EmployeeId,
            AccountingDate = salesReturn.AccountingDate,
            DocumentDate   = salesReturn.DocumentDate,
            Description    = salesReturn.Description,
            DeliveryPerson = salesReturn.Customer?.Name,
            Reference      = salesReturn.DocumentNumber,
            TotalAmount    = salesReturn.Lines.Sum(l => l.CostAmount),
            CreatedAt      = DateTime.UtcNow,
            ConfirmedAt    = DateTime.UtcNow,
            Lines          = salesReturn.Lines.Select(l => new WarehouseReceiptLine
            {
                ProductId     = l.ProductId,
                WarehouseId   = l.WarehouseId,
                Quantity      = l.Quantity,
                UnitPrice     = l.CostPrice,
                Amount        = l.CostAmount,
                DebitAccount  = l.CostAccount,
                CreditAccount = l.CogsAccount,
            }).ToList(),
        };

        var saved = await _receiptRepo.AddAsync(receipt, ct);

        _logger.LogInformation(
            "Created & auto-confirmed WarehouseReceipt {ReceiptNumber} (ReturnedGoods) from SalesReturn {DocumentNumber} — stock not re-adjusted",
            receiptNumber, salesReturn.DocumentNumber);

        return CreateWarehouseReceiptUseCase.MapToDto(saved);
    }
}
