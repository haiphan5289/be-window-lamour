using Lamour.Api.Hubs;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Employees.Dtos;
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
}
