using Lamour.Api.Hubs;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Departments.Dtos;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.ExpenseCategories.Dtos;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Warehouses.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace Lamour.Api.Realtime;

public class SignalRNotificationBroadcaster : INotificationBroadcaster
{
    private readonly IHubContext<DataSyncHub> _hub;

    public SignalRNotificationBroadcaster(IHubContext<DataSyncHub> hub)
    {
        _hub = hub;
    }

    public Task CustomerCreatedAsync(CustomerResponseDto customer, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CustomerCreated", customer, ct);

    public Task CustomerUpdatedAsync(CustomerResponseDto customer, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CustomerUpdated", customer, ct);

    public Task CustomerDeletedAsync(int customerId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CustomerDeleted", customerId, ct);

    public Task CustomersBulkChangedAsync(CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CustomersBulkChanged", ct);

    public Task EmployeeCreatedAsync(EmployeeResponseDto employee, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("EmployeeCreated", employee, ct);

    public Task EmployeeUpdatedAsync(EmployeeResponseDto employee, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("EmployeeUpdated", employee, ct);

    public Task EmployeeDeletedAsync(int employeeId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("EmployeeDeleted", employeeId, ct);

    public Task ProductCreatedAsync(ProductResponseDto product, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductCreated", product, ct);

    public Task ProductUpdatedAsync(ProductResponseDto product, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductUpdated", product, ct);

    public Task ProductDeletedAsync(int productId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductDeleted", productId, ct);

    public Task SupplierCreatedAsync(SupplierResponseDto supplier, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("SupplierCreated", supplier, ct);

    public Task SupplierUpdatedAsync(SupplierResponseDto supplier, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("SupplierUpdated", supplier, ct);

    public Task SupplierDeletedAsync(int supplierId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("SupplierDeleted", supplierId, ct);

    public Task CategoryCreatedAsync(CategoryResponseDto category, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CategoryCreated", category, ct);

    public Task CategoryUpdatedAsync(CategoryResponseDto category, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CategoryUpdated", category, ct);

    public Task CategoryDeletedAsync(int categoryId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("CategoryDeleted", categoryId, ct);

    public Task ProductUnitCreatedAsync(ProductUnitResponseDto unit, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductUnitCreated", unit, ct);

    public Task ProductUnitUpdatedAsync(ProductUnitResponseDto unit, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductUnitUpdated", unit, ct);

    public Task ProductUnitDeletedAsync(int unitId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ProductUnitDeleted", unitId, ct);

    public Task AccountSettingCreatedAsync(AccountSettingResponseDto account, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("AccountSettingCreated", account, ct);

    public Task AccountSettingUpdatedAsync(AccountSettingResponseDto account, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("AccountSettingUpdated", account, ct);

    public Task AccountSettingDeletedAsync(int accountId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("AccountSettingDeleted", accountId, ct);

    public Task WarehouseCreatedAsync(WarehouseResponseDto warehouse, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("WarehouseCreated", warehouse, ct);

    public Task WarehouseUpdatedAsync(WarehouseResponseDto warehouse, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("WarehouseUpdated", warehouse, ct);

    public Task WarehouseDeletedAsync(int warehouseId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("WarehouseDeleted", warehouseId, ct);

    public Task DepartmentCreatedAsync(DepartmentResponseDto department, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("DepartmentCreated", department, ct);

    public Task DepartmentUpdatedAsync(DepartmentResponseDto department, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("DepartmentUpdated", department, ct);

    public Task DepartmentDeletedAsync(int departmentId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("DepartmentDeleted", departmentId, ct);

    public Task ExpenseCategoryCreatedAsync(ExpenseCategoryResponseDto category, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ExpenseCategoryCreated", category, ct);

    public Task ExpenseCategoryUpdatedAsync(ExpenseCategoryResponseDto category, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ExpenseCategoryUpdated", category, ct);

    public Task ExpenseCategoryDeletedAsync(int categoryId, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("ExpenseCategoryDeleted", categoryId, ct);
}
