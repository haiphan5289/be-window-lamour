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
//
// 2026-09-14 (đổi lại theo yêu cầu — "số NK phải cố định, chỉ tăng khi tạo BTL mới, sửa không
// tăng"): bản 09/11 chọn "lệch thì đánh dấu IsSuperseded + lập PN MỚI" (chấp nhận đổi số PN mỗi
// lần sửa) — user phản ánh số NK tăng nhanh hơn số BTL thật, gây khó theo dõi/đối chiếu. Giờ ExecuteAsync
// vẫn so khớp dòng hàng mỗi lần gọi: khớp thì tái dùng nguyên PN cũ (không đổi); LỆCH thì SỬA THẲNG
// dữ liệu (Lines/TotalAmount/...) của CHÍNH PN đó rồi lưu lại — giữ nguyên ReceiptNumber/Id, không
// tạo bản ghi mới, không đánh dấu IsSuperseded (field này vẫn giữ trong entity cho các bản ghi
// IsSuperseded=true đã lập từ trước 09/14 — không migration lùi, chỉ không set thêm nữa). AN TOÀN
// làm thẳng thế này vì PN loại ReturnedGoods này vốn KHÔNG cộng/trừ Product.StockQuantity bao giờ
// (xem comment class) — không cần đi qua Confirm/Unconfirm (nơi mới thật sự đụng tồn kho) nên không
// có rủi ro sai lệch tồn kho như đã cân nhắc & loại ở bản 09/11.
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

            // Chứng từ đã bị sửa sau khi lập PN — cập nhật lại THẲNG chính PN đó (giữ nguyên số PN),
            // không lập PN mới. `existingActive` đến từ GetAllAsync (AsNoTracking) nên phải nạp lại
            // TRACKED qua GetByIdAsync mới sửa được.
            var tracked = await _receiptRepo.GetByIdAsync(existingActive.Id, ct)
                ?? throw new NotFoundException($"WarehouseReceipt {existingActive.Id} not found.");

            tracked.AccountingDate = salesReturn.AccountingDate;
            tracked.DocumentDate   = salesReturn.DocumentDate;
            tracked.Description    = salesReturn.Description;
            tracked.DeliveryPerson = salesReturn.Customer?.Name;
            tracked.TotalAmount    = salesReturn.Lines.Sum(l => l.CostAmount);

            tracked.Lines.Clear();
            foreach (var l in salesReturn.Lines)
            {
                tracked.Lines.Add(new WarehouseReceiptLine
                {
                    ProductId     = l.ProductId,
                    WarehouseId   = l.WarehouseId,
                    Quantity      = l.Quantity,
                    UnitPrice     = l.CostPrice,
                    Amount        = l.CostAmount,
                    DebitAccount  = l.CostAccount,
                    CreditAccount = l.CogsAccount,
                });
            }

            await _receiptRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Updated WarehouseReceipt {ReceiptNumber} in place for SalesReturn {DocumentNumber} — lines changed after edit, receipt number kept unchanged",
                tracked.ReceiptNumber, salesReturn.DocumentNumber);

            return CreateWarehouseReceiptUseCase.MapToDto(tracked);
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
