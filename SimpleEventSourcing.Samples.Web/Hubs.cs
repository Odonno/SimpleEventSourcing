using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Web
{
    public class SyncHub<T> : Hub where T : class
    {
        public Task Sync(T item)
        {
            return Clients.All.SendAsync("Sync", item);
        }
    }

    public class CartHub : SyncHub<ItemAndQuantity> { }
    public class ItemHub : SyncHub<Item> { }
    public class OrderHub : SyncHub<Order> { }
    public class EventHub : SyncHub<SimpleEvent> { }
}
