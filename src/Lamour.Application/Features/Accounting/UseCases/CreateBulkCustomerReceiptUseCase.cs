using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// "Phiếu thu tiền khách hàng hàng loạt" — khớp ảnh mẫu MISA: tạo ĐÚNG 1 Receipt duy nhất (không
// group theo CustomerId ra nhiều phiếu như bản trước 2026-08-26), mỗi dòng hạch toán tự mang
// khách hàng riêng qua ReceiptEntry.SubjectCode/SubjectName (Receipt.CustomerId = null cho phiếu
// loại này — xem Receipt.cs). Tái dùng nguyên ICreateReceiptUseCase — validate còn nợ + ghi
// CashTransaction side-effect đã có sẵn ở đó, không viết lại.
public class CreateBulkCustomerReceiptUseCase : ICreateBulkCustomerReceiptUseCase
{
    private readonly ISalesOrderRepository      _salesOrderRepo;
    private readonly IEmployeeRepository        _employeeRepo;
    private readonly ICreateReceiptUseCase      _createReceipt;
    private readonly IGetNextReceiptCodeUseCase _getNextCode;
    private readonly ILogger<CreateBulkCustomerReceiptUseCase> _logger;

    public CreateBulkCustomerReceiptUseCase(
        ISalesOrderRepository      salesOrderRepo,
        IEmployeeRepository        employeeRepo,
        ICreateReceiptUseCase      createReceipt,
        IGetNextReceiptCodeUseCase getNextCode,
        ILogger<CreateBulkCustomerReceiptUseCase> logger)
    {
        _salesOrderRepo = salesOrderRepo;
        _employeeRepo   = employeeRepo;
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

        var payerName = request.PayerName;
        if (string.IsNullOrWhiteSpace(payerName))
        {
            var collector = request.CollectorEmployeeId.HasValue
                ? await _employeeRepo.GetByIdAsync(request.CollectorEmployeeId.Value, ct)
                : null;
            payerName = collector?.Name ?? "Thu tiền khách hàng hàng loạt";
        }

        var documentNumber = await _getNextCode.ExecuteAsync(ct);
        var reference       = string.Join(", ", ordersById.Values
            .Select(o => o.DocumentNumber)
            .Distinct());

        var createRequest = new CreateReceiptRequestDto
        {
            CustomerId          = null,
            PayerName           = payerName,
            Address             = request.Address,
            PaymentReason       = "ThuCongNo",
            CollectorEmployeeId = request.CollectorEmployeeId,
            Attachment          = request.Attachment,
            Reference           = reference,
            AccountingDate      = request.AccountingDate,
            DocumentDate        = request.DocumentDate,
            DocumentNumber      = documentNumber,
            Entries = request.Lines.Select(l =>
            {
                var order = ordersById[l.SalesOrderId];
                return new ReceiptEntryDto
                {
                    Description   = $"Thu tiền khách hàng - {order.DocumentNumber}",
                    DebitAccount  = request.DebitAccount,
                    CreditAccount = "Receivable131",
                    Amount        = l.Amount,
                    SubjectCode   = order.Customer.Code,
                    SubjectName   = order.CustomerNameOverride ?? order.Customer.Name,
                    BankAccount   = request.BankAccount,
                    SalesOrderId  = l.SalesOrderId,
                };
            }).ToList(),
        };

        var created = await _createReceipt.ExecuteAsync(createRequest, ct);

        _logger.LogInformation("Created bulk customer receipt {DocumentNumber} covering {LineCount} sales orders across {CustomerCount} customers",
            created.DocumentNumber, request.Lines.Count, ordersById.Values.Select(o => o.CustomerId).Distinct().Count());

        return new CreateBulkCustomerReceiptResponseDto { Receipt = created };
    }
}
