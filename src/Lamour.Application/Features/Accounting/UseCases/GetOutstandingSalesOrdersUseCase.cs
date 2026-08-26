using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// Danh sách Chứng từ bán hàng còn nợ khớp filter — nguồn dữ liệu cho popup "Thu tiền khách hàng
// hàng loạt" (tìm ngày → ra khách hàng đang còn nợ, tick chọn để thu tiền).
public class GetOutstandingSalesOrdersUseCase : IGetOutstandingSalesOrdersUseCase
{
    private readonly IReceiptRepository _repo;
    private readonly ILogger<GetOutstandingSalesOrdersUseCase> _logger;

    public GetOutstandingSalesOrdersUseCase(IReceiptRepository repo, ILogger<GetOutstandingSalesOrdersUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching outstanding sales orders from {From} to {To}, employee={EmployeeId}",
            fromDate, toDate, employeeId);

        var rows = await _repo.GetOutstandingSalesOrdersAsync(fromDate, toDate, employeeId, ct);

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
