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
//
// 2026-09-11 (bug fix "In ra sản phẩm cũ sau khi sửa chứng từ"): PN từng chỉ là 1 bản chụp
// (snapshot) dòng hàng tại thời điểm lập — nếu SalesReturn bị sửa SAU đó (thêm/đổi dòng), PN cũ
// không tự cập nhật, và "In" (PrintAsync ở WPF) cứ tìm thấy PN cũ là dùng luôn → in ra dữ liệu cũ.
// Giờ ExecuteAsync tự so khớp dòng hàng mỗi lần gọi: khớp thì tái dùng PN cũ (như trước); LỆCH thì
// đánh dấu PN cũ IsSuperseded=true (không xoá — chưa có API xoá, giữ lại để đối chiếu/audit) rồi
// lập PN MỚI khớp đúng dữ liệu hiện tại (số PN sẽ đổi — chấp nhận đánh đổi để luôn in đúng).
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
        var existingActive = existingReceipts.FirstOrDefault(r =>
            r.ReceiptType == WarehouseReceiptType.ReturnedGoods
            && r.Reference == salesReturn.DocumentNumber
            && !r.IsSuperseded);

        if (existingActive is not null)
        {
            var currentSignature  = BuildLineSignature(salesReturn.Lines.Select(l => (l.ProductId, l.WarehouseId, l.Quantity)));
            var existingSignature = BuildLineSignature(existingActive.Lines.Select(l => (l.ProductId, l.WarehouseId, l.Quantity)));

            if (currentSignature == existingSignature)
            {
                _logger.LogInformation(
                    "Reused existing WarehouseReceipt {ReceiptNumber} for SalesReturn {DocumentNumber} — lines unchanged",
                    existingActive.ReceiptNumber, salesReturn.DocumentNumber);
                return CreateWarehouseReceiptUseCase.MapToDto(existingActive);
            }

            // Chứng từ đã bị sửa sau khi lập PN — PN cũ không còn khớp dữ liệu hiện tại. `existingActive`
            // đến từ GetAllAsync (AsNoTracking) nên phải nạp lại TRACKED qua GetByIdAsync mới sửa được.
            var tracked = await _receiptRepo.GetByIdAsync(existingActive.Id, ct);
            if (tracked is not null)
            {
                tracked.IsSuperseded = true;
                await _receiptRepo.SaveChangesAsync(ct);
                _logger.LogInformation(
                    "Superseded WarehouseReceipt {ReceiptNumber} for SalesReturn {DocumentNumber} — lines no longer match after edit",
                    tracked.ReceiptNumber, salesReturn.DocumentNumber);
            }
        }

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

    private static string BuildLineSignature(IEnumerable<(int ProductId, int WarehouseId, int Quantity)> lines) =>
        string.Join("|", lines
            .OrderBy(l => l.ProductId).ThenBy(l => l.WarehouseId).ThenBy(l => l.Quantity)
            .Select(l => $"{l.ProductId}:{l.WarehouseId}:{l.Quantity}"));
}
