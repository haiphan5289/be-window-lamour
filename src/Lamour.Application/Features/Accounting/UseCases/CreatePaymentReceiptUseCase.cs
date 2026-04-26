using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class CreatePaymentReceiptUseCase : ICreatePaymentReceiptUseCase
{
    private readonly IPaymentReceiptRepository _receiptRepo;
    private readonly ICashLedgerRepository     _cashRepo;
    private readonly ICustomerRepository       _customerRepo;
    private readonly ILogger<CreatePaymentReceiptUseCase> _logger;

    public CreatePaymentReceiptUseCase(
        IPaymentReceiptRepository receiptRepo,
        ICashLedgerRepository cashRepo,
        ICustomerRepository customerRepo,
        ILogger<CreatePaymentReceiptUseCase> logger)
    {
        _receiptRepo  = receiptRepo;
        _cashRepo     = cashRepo;
        _customerRepo = customerRepo;
        _logger       = logger;
    }

    public async Task<PaymentReceiptResponseDto> ExecuteAsync(
        CreatePaymentReceiptRequestDto request, CancellationToken ct = default)
    {
        // Validate TotalAmount
        if (request.TotalAmount <= 0)
            throw new DomainException("TotalAmount must be greater than zero.");

        // Validate PaymentMethod
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var paymentMethod))
            throw new DomainException("PaymentMethod must be 'Cash' or 'BankTransfer'.");

        // Validate Customer exists
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, ct)
            ?? throw new DomainException($"Customer with id {request.CustomerId} not found.");

        // Generate receipt number
        var collectionDateUtc = DateTime.SpecifyKind(request.CollectionDate, DateTimeKind.Utc);
        var receiptNumber = await _receiptRepo.GetNextReceiptNumberAsync(collectionDateUtc, ct);

        // Build entity
        var receipt = new PaymentReceipt
        {
            ReceiptNumber  = receiptNumber,
            CustomerId     = request.CustomerId,
            EmployeeId     = request.EmployeeId,
            CollectionDate = collectionDateUtc,
            Description    = request.Description,
            TotalAmount    = request.TotalAmount,
            PaymentMethod  = paymentMethod,
            Currency       = request.Currency,
            ExchangeRate   = request.ExchangeRate,
            CreatedAt      = DateTime.UtcNow,
            Lines          = request.Lines.Select(l => new PaymentReceiptLine
            {
                DocumentDate   = DateTime.SpecifyKind(l.DocumentDate, DateTimeKind.Utc),
                DocumentNumber = l.DocumentNumber,
                InvoiceNumber  = l.InvoiceNumber,
                Description    = l.Description,
                DueDate        = l.DueDate.HasValue
                                     ? DateTime.SpecifyKind(l.DueDate.Value, DateTimeKind.Utc)
                                     : null,
                AmountDue  = l.AmountDue,
                AmountPaid = l.AmountPaid,
            }).ToList()
        };

        var saved = await _receiptRepo.AddAsync(receipt, ct);

        // Auto-create CashTransaction
        var cashTx = new CashTransaction
        {
            AccountingDate = collectionDateUtc,
            DocumentDate   = collectionDateUtc,
            ReceiptNumber  = receiptNumber,
            PaymentNumber  = null,
            Description    = !string.IsNullOrWhiteSpace(request.Description)
                                 ? request.Description
                                 : $"Thu tiền khách hàng {customer.Name}",
            Account        = "111",
            CounterAccount = "131",
            DebitAmount    = request.TotalAmount,
            CreditAmount   = 0m,
            PersonName     = saved.Employee?.Name ?? customer.Name,
            CreatedAt      = DateTime.UtcNow,
        };

        await _cashRepo.AddAsync(cashTx, ct);

        _logger.LogInformation(
            "Created PaymentReceipt {ReceiptNumber} for customer {CustomerId}",
            receiptNumber, request.CustomerId);

        return MapToDto(saved);
    }

    private static PaymentReceiptResponseDto MapToDto(PaymentReceipt r) => new()
    {
        Id             = r.Id,
        ReceiptNumber  = r.ReceiptNumber,
        CustomerId     = r.CustomerId,
        CustomerName   = r.Customer?.Name ?? "",
        EmployeeId     = r.EmployeeId,
        EmployeeName   = r.Employee?.Name,
        CollectionDate = r.CollectionDate,
        TotalAmount    = r.TotalAmount,
        PaymentMethod  = r.PaymentMethod.ToString(),
        Currency       = r.Currency,
        ExchangeRate   = r.ExchangeRate,
        CreatedAt      = r.CreatedAt,
        Lines          = r.Lines.Select(l => new PaymentReceiptLineDto
        {
            Id             = l.Id,
            DocumentDate   = l.DocumentDate,
            DocumentNumber = l.DocumentNumber,
            InvoiceNumber  = l.InvoiceNumber,
            Description    = l.Description,
            DueDate        = l.DueDate,
            AmountDue      = l.AmountDue,
            AmountPaid     = l.AmountPaid,
        }).ToList()
    };
}
