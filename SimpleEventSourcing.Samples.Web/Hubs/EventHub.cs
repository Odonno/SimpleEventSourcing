using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Web.Hubs
{
    public class EventHub : Hub
    {
        public Task Sync(SimpleEvent @event)
        {
            return Clients.All.SendAsync("Sync", @event);
        }
    }
}
