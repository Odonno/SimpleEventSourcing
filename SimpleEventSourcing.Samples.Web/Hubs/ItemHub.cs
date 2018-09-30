using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Web.Hubs
{
    public class ItemHub : Hub
    {
        public Task Sync(Item item)
        {
            return Clients.All.SendAsync("Sync", item);
        }
    }
}
