using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class UpdatePaymentUseCase : IUpdatePaymentUseCase
{
    private readonly IPaymentRepository         _repo;
    private readonly IExpenseCategoryRepository _expenseCategoryRepo;
    private readonly IAccountSettingRepository  _accountSettingRepo;
    private readonly ISupplierRepository        _supplierRepo;
    private readonly ICustomerRepository        _customerRepo;
    private readonly IEmployeeRepository        _employeeRepo;
    private readonly ILogger<UpdatePaymentUseCase> _logger;

    public UpdatePaymentUseCase(
        IPaymentRepository repo,
        IExpenseCategoryRepository expenseCategoryRepo,
        IAccountSettingRepository accountSettingRepo,
        ISupplierRepository supplierRepo,
        ICustomerRepository customerRepo,
        IEmployeeRepository employeeRepo,
        ILogger<UpdatePaymentUseCase> logger)
    {
        _repo                = repo;
        _expenseCategoryRepo = expenseCategoryRepo;
        _accountSettingRepo  = accountSettingRepo;
        _supplierRepo        = supplierRepo;
        _customerRepo        = customerRepo;
        _employeeRepo        = employeeRepo;
        _logger              = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(
        int id, UpdatePaymentRequestDto request, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        if (payment.Status == PaymentStatus.Confirmed)
            throw new DomainException("Phiếu chi đã ghi số, không thể sửa. Vui lòng hủy ghi số trước.");

        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'.");

        if (!Enum.TryParse<PaymentPartnerType>(request.PartnerType, out var partnerType))
            throw new DomainException($"Invalid partner_type '{request.PartnerType}'. Valid values: Supplier, Customer, Employee.");

        var partnerName = await PaymentPartnerResolver.ResolveNameAsync(
            partnerType, request.PartnerId, _supplierRepo, _customerRepo, _employeeRepo, ct);

        // Update header fields
        payment.PartnerType       = partnerType;
        payment.PartnerId         = request.PartnerId;
        payment.PartnerName       = partnerName;
        payment.PayeeName         = request.PayeeName;
        payment.Address           = request.Address;
        payment.PaymentReason     = paymentReason;
        payment.ReasonDetail      = string.IsNullOrWhiteSpace(request.ReasonDetail) ? null : request.ReasonDetail.Trim();
        payment.PaymentEmployeeId = request.PaymentEmployeeId;
        payment.Attachment        = request.Attachment;
        payment.Reference         = request.Reference;
        payment.AccountingDate    = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        payment.DocumentDate      = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc);
        payment.DocumentNumber    = request.DocumentNumber;

        // Replace entries
        payment.Entries.Clear();
        foreach (var e in request.Entries)
        {
            if (await _accountSettingRepo.GetByIdAsync(e.DebitAccountId, ct) is null)
                throw new DomainException("Tài khoản Nợ không tồn tại.");
            if (await _accountSettingRepo.GetByIdAsync(e.CreditAccountId, ct) is null)
                throw new DomainException("Tài khoản Có không tồn tại.");
            if (e.ExpenseCategoryId is not null && await _expenseCategoryRepo.GetByIdAsync(e.ExpenseCategoryId.Value, ct) is null)
                throw new DomainException("Khoản mục chi phí không tồn tại.");

            payment.Entries.Add(new PaymentEntry
            {
                Description             = e.Description,
                DebitAccountSettingId    = e.DebitAccountId,
                CreditAccountSettingId   = e.CreditAccountId,
                Amount                   = e.Amount,
                SubjectCode              = e.SubjectCode,
                SubjectName              = e.SubjectName,
                BankAccount              = e.BankAccount,
                ExpenseCategoryId        = e.ExpenseCategoryId,
            });
        }

        await _repo.UpdateAsync(payment, ct);

        _logger.LogInformation("Updated Payment {Id} ({DocumentNumber})", id, payment.DocumentNumber);

        // Re-fetch untracked with navigations included so the response reflects account/expense-category names.
        var refreshed = await _repo.GetByIdAsync(id, ct) ?? payment;
        return GetPaymentsUseCase.MapToDto(refreshed);
    }
}
