using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Warehouses.Dtos;

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

    Task CategoryCreatedAsync(CategoryResponseDto category, CancellationToken ct = default);
    Task CategoryUpdatedAsync(CategoryResponseDto category, CancellationToken ct = default);
    Task CategoryDeletedAsync(int categoryId, CancellationToken ct = default);

    Task ProductUnitCreatedAsync(ProductUnitResponseDto unit, CancellationToken ct = default);
    Task ProductUnitUpdatedAsync(ProductUnitResponseDto unit, CancellationToken ct = default);
    Task ProductUnitDeletedAsync(int unitId, CancellationToken ct = default);

    Task AccountSettingCreatedAsync(AccountSettingResponseDto account, CancellationToken ct = default);
    Task AccountSettingUpdatedAsync(AccountSettingResponseDto account, CancellationToken ct = default);
    Task AccountSettingDeletedAsync(int accountId, CancellationToken ct = default);

    Task WarehouseCreatedAsync(WarehouseResponseDto warehouse, CancellationToken ct = default);
    Task WarehouseUpdatedAsync(WarehouseResponseDto warehouse, CancellationToken ct = default);
    Task WarehouseDeletedAsync(int warehouseId, CancellationToken ct = default);
}
