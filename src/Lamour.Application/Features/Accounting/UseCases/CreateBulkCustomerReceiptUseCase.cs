using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// "Phiếu thu tiền khách hàng hàng loạt" — nhận danh sách (SalesOrderId, Amount) đã chọn ở popup
// tìm kiếm, gom theo CustomerId (1 Receipt/khách hàng — khớp Receipt.CustomerId hiện là FK bắt
// buộc, không đổi schema Receipt), mỗi khách hàng có 1+ dòng nếu chọn nhiều đơn của cùng khách.
// Tái dùng nguyên ICreateReceiptUseCase cho từng Receipt — validate còn nợ + ghi CashTransaction
// side-effect đã có sẵn ở đó, không viết lại.
public class CreateBulkCustomerReceiptUseCase : ICreateBulkCustomerReceiptUseCase
{
    private readonly ISalesOrderRepository      _salesOrderRepo;
    private readonly ICreateReceiptUseCase      _createReceipt;
    private readonly IGetNextReceiptCodeUseCase _getNextCode;
    private readonly ILogger<CreateBulkCustomerReceiptUseCase> _logger;

    public CreateBulkCustomerReceiptUseCase(
        ISalesOrderRepository      salesOrderRepo,
        ICreateReceiptUseCase      createReceipt,
        IGetNextReceiptCodeUseCase getNextCode,
        ILogger<CreateBulkCustomerReceiptUseCase> logger)
    {
        _salesOrderRepo = salesOrderRepo;
        _createReceipt  = createReceipt;
        _getNextCode    = getNextCode;
        _logger         = logger;
    }

    public async Task<CreateBulkCustomerReceiptResponseDto> ExecuteAsync(
        CreateBulkCustomerReceiptRequestDto request, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("Phải chọn ít nhất 1 chứng từ để thu tiền.");

        var ordersById = new Dictionary<int, SalesOrder>();
        foreach (var line in request.Lines)
        {
            if (ordersById.ContainsKey(line.SalesOrderId)) continue;
            if (line.Amount <= 0)
                throw new DomainException($"Số tiền thu phải lớn hơn 0 (chứng từ id={line.SalesOrderId}).");

            var order = await _salesOrderRepo.GetByIdAsync(line.SalesOrderId, ct)
                ?? throw new DomainException($"Không tìm thấy chứng từ bán hàng id={line.SalesOrderId}.");
            ordersById[line.SalesOrderId] = order;
        }

        var results = new List<ReceiptResponseDto>();
        foreach (var group in request.Lines.GroupBy(l => ordersById[l.SalesOrderId].CustomerId))
        {
            var firstOrder      = ordersById[group.First().SalesOrderId];
            var documentNumber  = await _getNextCode.ExecuteAsync(ct);

            var createRequest = new CreateReceiptRequestDto
            {
                CustomerId          = group.Key,
                PayerName           = firstOrder.Customer.Name,
                Address             = null,
                PaymentReason       = "ThuCongNo",
                CollectorEmployeeId = request.CollectorEmployeeId,
                AccountingDate      = request.AccountingDate,
                DocumentDate        = request.DocumentDate,
                DocumentNumber      = documentNumber,
                Entries = group.Select(l => new ReceiptEntryDto
                {
                    Description   = $"Thu tiền khách hàng - {ordersById[l.SalesOrderId].DocumentNumber}",
                    DebitAccount  = request.DebitAccount,
                    CreditAccount = "Receivable131",
                    Amount        = l.Amount,
                    BankAccount   = request.BankAccount,
                    SalesOrderId  = l.SalesOrderId,
                }).ToList(),
            };

            var created = await _createReceipt.ExecuteAsync(createRequest, ct);
            results.Add(created);
        }

        _logger.LogInformation("Created {Count} bulk customer receipts covering {LineCount} sales orders",
            results.Count, request.Lines.Count);

        return new CreateBulkCustomerReceiptResponseDto { Receipts = results };
    }
}
