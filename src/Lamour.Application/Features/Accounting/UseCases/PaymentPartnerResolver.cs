using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;

namespace Lamour.Application.Features.Accounting.UseCases;

/// <summary>
/// Validates a Payment's polymorphic "Đối tượng" reference (PartnerType + PartnerId) against
/// the matching master-data table and resolves the display name to cache on the Payment row.
/// </summary>
internal static class PaymentPartnerResolver
{
    public static async Task<string> ResolveNameAsync(
        PaymentPartnerType partnerType,
        int partnerId,
        ISupplierRepository supplierRepo,
        ICustomerRepository customerRepo,
        IEmployeeRepository employeeRepo,
        CancellationToken ct)
    {
        switch (partnerType)
        {
            case PaymentPartnerType.Supplier:
                var supplier = await supplierRepo.GetByIdAsync(partnerId, ct)
                    ?? throw new DomainException("Nhà cung cấp không tồn tại.");
                return supplier.Name;

            case PaymentPartnerType.Customer:
                var customer = await customerRepo.GetByIdAsync(partnerId, ct)
                    ?? throw new DomainException("Khách hàng không tồn tại.");
                return customer.Name;

            case PaymentPartnerType.Employee:
                var employee = await employeeRepo.GetByIdAsync(partnerId, ct)
                    ?? throw new DomainException("Nhân viên không tồn tại.");
                return employee.Name;

            default:
                throw new DomainException($"Loại đối tượng '{partnerType}' không hợp lệ.");
        }
    }
}
