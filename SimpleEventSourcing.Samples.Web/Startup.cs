//using System;
//using System.Collections.Generic;
//using System.Dynamic;
using System.IO;
//using System.Linq;
//using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;
//using Swashbuckle.AspNetCore.Swagger;
//using Tagada.Swagger;
//using static SimpleEventSourcing.Samples.Web.Database.Configuration;
//using static SimpleEventSourcing.Samples.Web.Program;

namespace SimpleEventSourcing.Samples.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc()
            //    .AddJsonOptions(o =>
            //        o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
            //        {
            //            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true }
            //        }
            //    );

            //services.AddRouting();

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info
            //    {
            //        Title = "SimpleEventSourcing Example API",
            //        Version = "v1"
            //    });

            //    c.GenerateTagadaSwaggerDoc();
            //});

            //services.AddSignalR();
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env)//,
            //IHubContext<CartHub> cartHubContext,
            //IHubContext<OrderHub> orderHubContext,
            //IHubContext<ItemHub> itemHubContext,
            //IHubContext<EventHub> eventHubContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    ConfigFile = "webpack.config.js",
                    HotModuleReplacement = true,
                    ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "app")
                });
            }

            //// Handle database creation
            //CreateEventsDatabase();
            //CreateViewsDatabase();

            app.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = env.IsDevelopment(),
                EnableDefaultFiles = true
            });

            //// Realtime connections
            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<CartHub>("/cart");
            //    routes.MapHub<OrderHub>("/order");
            //    routes.MapHub<ItemHub>("/item");
            //    routes.MapHub<EventHub>("/event");
            //});

            //app.UseMvc();

            //// API routes
            //app.Map("/api")
            //    .Post<ReplayEventsCommand>("/event/replay", (_) => ReplayEvents(cartHubContext, itemHubContext, orderHubContext))
            //    .AddSwagger()
            //    .Use();

            //// Swagger UI
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("v1/swagger.json", "SimpleEventSourcing Example API V1");
            //});

            //// Synchronize backend events (data mutations) with clients using signalr
            //OrderEventView.ObserveEntityChange().Subscribe(async order =>
            //{
            //    await orderHubContext.Clients.All.SendAsync("Sync", order);
            //});

            //CartEventView.ObserveEntityChange().Subscribe(async itemAndQuantity =>
            //{
            //    await cartHubContext.Clients.All.SendAsync("Sync", itemAndQuantity);
            //});

            //ItemEventView.ObserveEntityChange().Subscribe(async item =>
            //{
            //    await itemHubContext.Clients.All.SendAsync("Sync", item);
            //});

            //AppEventStore.ObserveEvent().Subscribe(async @event =>
            //{
            //    await eventHubContext.Clients.All.SendAsync("Sync", @event);
            //});
        }      

    //    public static Action<IHubContext<CartHub>, IHubContext<ItemHub>, IHubContext<OrderHub>> ReplayEvents = async (
    //        cartHubContext,
    //        itemHubContext,
    //        orderHubContext) =>
    //    {
    //        // Get events stored
    //        IEnumerable<SimpleEvent> events;
    //        using (var connection = GetEventsDatabaseConnection())
    //        {
    //            events = connection
    //                .Query<EventDbo>("SELECT * FROM [Event] ORDER BY [Id] ASC")
    //                .Select(eventDbo =>
    //                {
    //                    return new AppEvent
    //                    {
    //                        Id = eventDbo.Id,
    //                        EventName = eventDbo.EventName,
    //                        Data = JsonConvert.DeserializeObject(eventDbo.Data),
    //                        Metadata = JsonConvert.DeserializeObject(eventDbo.Metadata)
    //                    };
    //                })
    //                .ToList();
    //        }

    //        // Clear views database
    //        using (var connection = GetViewsDatabaseConnection())
    //        {
    //            connection.Execute(
    //                @"
    //                DELETE FROM [ItemOrdered];
    //                DELETE FROM [Cart];
    //                DELETE FROM [Order];
    //                DELETE FROM [Item];
    //                DELETE FROM [sqlite_sequence];
    //                "
    //            );
    //        }

    //        // Replay events
    //        foreach (var @event in events)
    //        {
    //            CartEventView.Replay(@event);
    //            ItemEventView.Replay(@event);
    //            OrderEventView.Replay(@event);
    //        }

    //        List<Item> inventoryItems;
    //        List<ItemAndQuantity> cartItems;
    //        List<Order> orders;

    //        // Get results from the replay in realtime
    //        using (var connection = GetViewsDatabaseConnection())
    //        {
    //            inventoryItems = connection
    //                .Query<ItemDbo>("SELECT * FROM [Item]")
    //                .Select(item =>
    //                {
    //                    return new Item
    //                    {
    //                        Id = item.Id,
    //                        Name = item.Name,
    //                        Price = Convert.ToDecimal(item.Price),
    //                        RemainingQuantity = item.RemainingQuantity,
    //                    };
    //                })
    //                .ToList();

    //            cartItems = connection
    //                .Query<ItemAndQuantity>("SELECT [ItemId], [Quantity] FROM [Cart]")
    //                .ToList();

    //            var ordersDbo = connection
    //                .Query<OrderDbo>("SELECT * FROM [Order]")
    //                .ToList();
    //            var itemsOrderedDbo = connection
    //                .Query<ItemOrderedDbo>("SELECT * FROM [ItemOrdered]")
    //                .ToList();

    //            orders = ordersDbo
    //                .Select(order =>
    //                {
    //                    return new Order
    //                    {
    //                        Id = order.Id,
    //                        CreatedDate = order.CreatedDate,
    //                        Number = order.Number,
    //                        IsConfirmed = order.IsConfirmed,
    //                        IsCanceled = order.IsCanceled,
    //                        Items = itemsOrderedDbo
    //                            .Where(i => i.OrderId == order.Id)
    //                            .Select(i =>
    //                            {
    //                                return new ItemAndPriceAndQuantity
    //                                {
    //                                    ItemId = i.ItemId,
    //                                    Price = Convert.ToDecimal(i.Price),
    //                                    Quantity = i.Quantity
    //                                };
    //                            })
    //                    };
    //                })
    //                .ToList();
    //        }

    //        // Sync data with the client
    //        foreach (var item in inventoryItems)
    //        {
    //            await itemHubContext.Clients.All.SendAsync("Sync", item);
    //        }

    //        foreach (var cartItem in cartItems)
    //        {
    //            await cartHubContext.Clients.All.SendAsync("Sync", cartItem);
    //        }

    //        foreach (var order in orders)
    //        {
    //            await orderHubContext.Clients.All.SendAsync("Sync", order);
    //        }
    //    };
    }
}
