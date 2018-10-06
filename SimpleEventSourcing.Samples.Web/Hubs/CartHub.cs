using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Web.Hubs
{
    public class CartHub : Hub
    {
        public Task Sync(ItemAndQuantity itemAndQuantity)
        {
            return Clients.All.SendAsync("Sync", itemAndQuantity);
        }
    }
}
