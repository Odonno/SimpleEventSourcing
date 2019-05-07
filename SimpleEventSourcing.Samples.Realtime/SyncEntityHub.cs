using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.Realtime
{
    public class SyncEntityHub<TEntity> : Hub where TEntity : class
    {
        public Task Sync(TEntity item)
        {
            return Clients.All.SendAsync("Sync", item);
        }
    }
}
