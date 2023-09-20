using Microsoft.AspNetCore.SignalR;
using Shared;

namespace Monitor.Hubs
{
    public class FactoryInfoHub : Hub<IFactoryInfoHub>
    {
        public async Task Send(FactoryInfo info)
        {
            await Clients.All.SendInfo(info);
        }
    }
}
