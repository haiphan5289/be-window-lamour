using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class CreateReceiptUseCase : ICreateReceiptUseCase
{
    private readonly IReceiptRepository        _repo;
    private readonly IAccountSettingRepository _accountSettingRepo;
    private readonly ICustomerRepository       _customerRepo;
    private readonly IEmployeeRepository       _employeeRepo;
    private readonly ILogger<CreateReceiptUseCase> _logger;

    public CreateReceiptUseCase(
        IReceiptRepository repo,
        IAccountSettingRepository accountSettingRepo,
        ICustomerRepository customerRepo,
        IEmployeeRepository employeeRepo,
        ILogger<CreateReceiptUseCase> logger)
    {
        _repo               = repo;
        _accountSettingRepo = accountSettingRepo;
        _customerRepo       = customerRepo;
        _employeeRepo       = employeeRepo;
        _logger             = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        CreateReceiptRequestDto request, CancellationToken ct = default)
    {
        var paymentReason = ReceiptEntryBuilder.ParseReason(request.PaymentReason);
        var partner = await ReceiptEntryBuilder.ResolvePartnerAsync(
            request.PartnerType, request.PartnerId, paymentReason, _customerRepo, _employeeRepo, ct);
        var entries = await ReceiptEntryBuilder.BuildEntriesAsync(
            request.Entries, _repo, _accountSettingRepo, validateRemaining: true, ct);

        var receipt = new Receipt
        {
            PayerName           = request.PayerName,
            Address             = request.Address,
            PaymentReason       = paymentReason,
            ReasonDetail        = string.IsNullOrWhiteSpace(request.ReasonDetail) ? null : request.ReasonDetail.Trim(),
            CollectorEmployeeId = request.CollectorEmployeeId,
            Attachment          = request.Attachment,
            Reference           = request.Reference,
            AccountingDate      = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate        = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc),
            DocumentNumber      = request.DocumentNumber,
            CreatedAt           = DateTime.UtcNow,
            Entries             = entries,
        };

        ReceiptEntryBuilder.ApplyPartner(receipt, partner);

        var saved = await _repo.AddAsync(receipt, ct);

        // Draft receipt has no cash-ledger effect yet — CashTransaction is now posted only on
        // Confirm ("Ghi sổ"), mirroring Payment/SalesReturn. See ConfirmReceiptUseCase.

        _logger.LogInformation("Created Receipt {DocumentNumber} for {PartnerType} {PartnerId}",
            saved.DocumentNumber, saved.PartnerType, saved.PartnerId);

        return GetReceiptsUseCase.MapToDto(saved);
    }
}
