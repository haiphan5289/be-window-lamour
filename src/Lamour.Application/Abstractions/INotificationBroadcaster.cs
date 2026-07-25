using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Suppliers.Dtos;

namespace Lamour.Application.Abstractions;

public interface INotificationBroadcaster
{
    Task CustomerCreatedAsync(CustomerResponseDto customer, CancellationToken ct = default);
    Task CustomerUpdatedAsync(CustomerResponseDto customer, CancellationToken ct = default);
    Task CustomerDeletedAsync(int customerId, CancellationToken ct = default);
    Task CustomersBulkChangedAsync(CancellationToken ct = default);

    Task EmployeeCreatedAsync(EmployeeResponseDto employee, CancellationToken ct = default);
    Task EmployeeUpdatedAsync(EmployeeResponseDto employee, CancellationToken ct = default);
    Task EmployeeDeletedAsync(int employeeId, CancellationToken ct = default);

    Task ProductCreatedAsync(ProductResponseDto product, CancellationToken ct = default);
    Task ProductUpdatedAsync(ProductResponseDto product, CancellationToken ct = default);
    Task ProductDeletedAsync(int productId, CancellationToken ct = default);

    Task SupplierCreatedAsync(SupplierResponseDto supplier, CancellationToken ct = default);
    Task SupplierUpdatedAsync(SupplierResponseDto supplier, CancellationToken ct = default);
    Task SupplierDeletedAsync(int supplierId, CancellationToken ct = default);
}
