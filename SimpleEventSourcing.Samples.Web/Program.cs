using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SimpleEventSourcing.Samples.Web.Database;

namespace SimpleEventSourcing.Samples.Web
{
    public static class Program
    {
        public static readonly AppCommandDispatcher AppCommandDispatcher = new AppCommandDispatcher();

        public static readonly AppEventStore AppEventStore = new AppEventStore(AppCommandDispatcher.ObserveEventAggregate());

        public static readonly CartEventView CartEventView = new CartEventView(AppEventStore.ObserveEvent());
        public static readonly ItemEventView ItemEventView = new ItemEventView(AppEventStore.ObserveEvent());
        public static readonly OrderEventView OrderEventView = new OrderEventView(AppEventStore.ObserveEvent());

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
