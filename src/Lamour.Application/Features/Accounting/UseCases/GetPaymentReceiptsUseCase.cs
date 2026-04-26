using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetPaymentReceiptsUseCase : IGetPaymentReceiptsUseCase
{
    private readonly IPaymentReceiptRepository _repo;
    private readonly ILogger<GetPaymentReceiptsUseCase> _logger;

    public GetPaymentReceiptsUseCase(
        IPaymentReceiptRepository repo,
        ILogger<GetPaymentReceiptsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var receipts = await _repo.GetAllAsync(ct);

        return receipts
            .OrderByDescending(r => r.CollectionDate)
            .Select(r => new PaymentReceiptResponseDto
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
            });
    }
}
