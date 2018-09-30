using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Web.Hubs
{
    public class OrderHub : Hub
    {
        public Task Sync(Order order)
        {
            return Clients.All.SendAsync("Sync", order);
        }
    }
}
