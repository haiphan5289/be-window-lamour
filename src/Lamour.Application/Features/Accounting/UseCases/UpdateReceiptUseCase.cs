using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class UpdateReceiptUseCase : IUpdateReceiptUseCase
{
    private readonly IReceiptRepository        _repo;
    private readonly IAccountSettingRepository _accountSettingRepo;
    private readonly ICustomerRepository       _customerRepo;
    private readonly IEmployeeRepository       _employeeRepo;
    private readonly ILogger<UpdateReceiptUseCase> _logger;

    public UpdateReceiptUseCase(
        IReceiptRepository repo,
        IAccountSettingRepository accountSettingRepo,
        ICustomerRepository customerRepo,
        IEmployeeRepository employeeRepo,
        ILogger<UpdateReceiptUseCase> logger)
    {
        _repo               = repo;
        _accountSettingRepo = accountSettingRepo;
        _customerRepo       = customerRepo;
        _employeeRepo       = employeeRepo;
        _logger             = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        int id, UpdateReceiptRequestDto request, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (receipt.Status != ReceiptStatus.Draft)
            throw new DomainException("Chứng từ đã ghi sổ, không thể sửa. Bỏ ghi trước khi sửa.");

        var paymentReason = ReceiptEntryBuilder.ParseReason(request.PaymentReason);
        var partner = await ReceiptEntryBuilder.ResolvePartnerAsync(
            request.PartnerType, request.PartnerId, paymentReason, _customerRepo, _employeeRepo, ct);
        // Validate mọi dòng TRƯỚC khi đụng vào entity đang tracked.
        var entries = await ReceiptEntryBuilder.BuildEntriesAsync(
            request.Entries, _repo, _accountSettingRepo, validateRemaining: false, ct);

        // Update header fields
        ReceiptEntryBuilder.ApplyPartner(receipt, partner);
        receipt.PayerName           = request.PayerName;
        receipt.Address             = request.Address;
        receipt.PaymentReason       = paymentReason;
        receipt.ReasonDetail        = string.IsNullOrWhiteSpace(request.ReasonDetail) ? null : request.ReasonDetail.Trim();
        receipt.CollectorEmployeeId = request.CollectorEmployeeId;
        receipt.Attachment          = request.Attachment;
        receipt.Reference           = request.Reference;
        receipt.AccountingDate      = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        receipt.DocumentDate        = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc);
        receipt.DocumentNumber      = request.DocumentNumber;

        // Replace entries
        receipt.Entries.Clear();
        foreach (var entry in entries) receipt.Entries.Add(entry);

        await _repo.UpdateAsync(receipt, ct);

        // Draft receipt never had a CashTransaction — nothing to sync here anymore. Cash-ledger
        // posting now happens only on Confirm ("Ghi sổ"). See ConfirmReceiptUseCase.

        _logger.LogInformation("Updated Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);

        // Nạp lại để dòng mới có đủ mã/tên TK (navigation chưa được load trên entity vừa thêm).
        var refreshed = await _repo.GetByIdAsync(id, ct) ?? receipt;
        return GetReceiptsUseCase.MapToDto(refreshed);
    }
}
