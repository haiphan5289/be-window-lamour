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

public class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentRepository         _repo;
    private readonly IExpenseCategoryRepository _expenseCategoryRepo;
    private readonly IAccountSettingRepository  _accountSettingRepo;
    private readonly ISupplierRepository        _supplierRepo;
    private readonly ICustomerRepository        _customerRepo;
    private readonly IEmployeeRepository        _employeeRepo;
    private readonly ILogger<CreatePaymentUseCase> _logger;

    public CreatePaymentUseCase(
        IPaymentRepository repo,
        IExpenseCategoryRepository expenseCategoryRepo,
        IAccountSettingRepository accountSettingRepo,
        ISupplierRepository supplierRepo,
        ICustomerRepository customerRepo,
        IEmployeeRepository employeeRepo,
        ILogger<CreatePaymentUseCase> logger)
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
        CreatePaymentRequestDto request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'. Valid values: ChiKhac, ChiMuaHang, ChiTraNo, ChiLuong.");

        if (!Enum.TryParse<PaymentPartnerType>(request.PartnerType, out var partnerType))
            throw new DomainException($"Invalid partner_type '{request.PartnerType}'. Valid values: Supplier, Customer, Employee.");

        var partnerName = await PaymentPartnerResolver.ResolveNameAsync(
            partnerType, request.PartnerId, _supplierRepo, _customerRepo, _employeeRepo, ct);

        var entries = new List<PaymentEntry>();
        foreach (var e in request.Entries)
        {
            if (await _accountSettingRepo.GetByIdAsync(e.DebitAccountId, ct) is null)
                throw new DomainException("Tài khoản Nợ không tồn tại.");
            if (await _accountSettingRepo.GetByIdAsync(e.CreditAccountId, ct) is null)
                throw new DomainException("Tài khoản Có không tồn tại.");
            if (e.ExpenseCategoryId is not null && await _expenseCategoryRepo.GetByIdAsync(e.ExpenseCategoryId.Value, ct) is null)
                throw new DomainException("Khoản mục chi phí không tồn tại.");

            entries.Add(new PaymentEntry
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

        var payment = new Payment
        {
            PartnerType       = partnerType,
            PartnerId         = request.PartnerId,
            PartnerName       = partnerName,
            PayeeName         = request.PayeeName,
            Address           = request.Address,
            PaymentReason     = paymentReason,
            ReasonDetail      = string.IsNullOrWhiteSpace(request.ReasonDetail) ? null : request.ReasonDetail.Trim(),
            PaymentEmployeeId = request.PaymentEmployeeId,
            Attachment        = request.Attachment,
            Reference         = request.Reference,
            AccountingDate    = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate      = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc),
            DocumentNumber    = request.DocumentNumber,
            Status            = PaymentStatus.Draft,
            CreatedAt         = DateTime.UtcNow,
            Entries           = entries,
        };

        var saved = await _repo.AddAsync(payment, ct);

        _logger.LogInformation("Created Payment {DocumentNumber} for {PartnerType} {PartnerId} (Draft)",
            saved.DocumentNumber, saved.PartnerType, saved.PartnerId);

        return GetPaymentsUseCase.MapToDto(saved);
    }
}
