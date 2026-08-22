using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetReceiptsUseCase : IGetReceiptsUseCase
{
    private readonly IReceiptRepository _repo;
    private readonly ILogger<GetReceiptsUseCase> _logger;

    public GetReceiptsUseCase(IReceiptRepository repo, ILogger<GetReceiptsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<ReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all receipts");
        var receipts = await _repo.GetAllAsync(ct);
        return receipts.Select(MapToDto);
    }

    internal static ReceiptResponseDto MapToDto(Domain.Entities.Receipt r) => new()
    {
        Id                    = r.Id,
        CustomerId            = r.CustomerId,
        CustomerName          = r.Customer?.Name ?? "",
        PayerName             = r.PayerName,
        Address               = r.Address,
        PaymentReason         = r.PaymentReason.ToString(),
        CollectorEmployeeId   = r.CollectorEmployeeId,
        CollectorEmployeeName = r.CollectorEmployee?.Name,
        Attachment            = r.Attachment,
        Reference             = r.Reference,
        AccountingDate        = r.AccountingDate,
        DocumentDate          = r.DocumentDate,
        DocumentNumber        = r.DocumentNumber,
        CreatedAt             = r.CreatedAt,
        Entries               = r.Entries.Select(e => new ReceiptEntryDto
        {
            Id            = e.Id,
            Description   = e.Description,
            DebitAccount  = e.DebitAccount.ToString(),
            CreditAccount = e.CreditAccount.ToString(),
            Amount        = e.Amount,
            SubjectCode   = e.SubjectCode,
            SubjectName   = e.SubjectName,
            BankAccount   = e.BankAccount,
            SalesOrderId  = e.SalesOrderId,
        }).ToList(),
    };
}
