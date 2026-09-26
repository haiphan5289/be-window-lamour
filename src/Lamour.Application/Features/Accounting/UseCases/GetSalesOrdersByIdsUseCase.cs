using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// Dùng để dựng lại tab "2. Chứng từ" khi Sửa 1 phiếu thu hàng loạt đã lưu — reload đúng các
// SalesOrder đã gắn vào phiếu (bất kể còn nợ hay không, xem doc comment IReceiptRepository).
// RemainingAmount trả về là số còn nợ THẬT NGAY LÚC GỌI (đã trừ cả entry của chính phiếu đang sửa,
// giống GetRemainingAmountAsync) — WPF tự cộng lại Amount hiện có của dòng đang sửa để ra đúng
// "Số chưa thu" tối đa cho phép nhập (remaining thật + số tiền dòng này đang giữ).
public class GetSalesOrdersByIdsUseCase : IGetSalesOrdersByIdsUseCase
{
    private readonly IReceiptRepository _repo;
    private readonly ILogger<GetSalesOrdersByIdsUseCase> _logger;

    public GetSalesOrdersByIdsUseCase(IReceiptRepository repo, ILogger<GetSalesOrdersByIdsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default)
    {
        var idList = salesOrderIds.ToList();
        _logger.LogInformation("Fetching {Count} sales orders by id for bulk receipt edit", idList.Count);

        var rows = await _repo.GetSalesOrdersByIdsAsync(idList, ct);

        return rows.Select(r => new OutstandingSalesOrderDto
        {
            SalesOrderId    = r.OrderId,
            DocumentNumber  = r.DocumentNumber,
            AccountingDate  = r.AccountingDate,
            DocumentDate    = r.DocumentDate,
            CustomerId      = r.CustomerId,
            CustomerCode    = r.CustomerCode,
            CustomerName    = r.CustomerName,
            Description     = r.Description,
            RemainingAmount = r.RemainingAmount,
            GrandTotal      = r.GrandTotal,
            PaymentTerms    = r.PaymentTerms,
            PaymentDueDate  = r.PaymentDueDate,
        });
    }
}
