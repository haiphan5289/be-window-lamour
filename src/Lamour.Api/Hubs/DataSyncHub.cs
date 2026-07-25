using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Lamour.Api.Hubs;

[Authorize]
public class DataSyncHub : Hub
{
}
