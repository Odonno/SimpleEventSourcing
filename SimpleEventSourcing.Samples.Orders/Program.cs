using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Tagada.Swagger;
using Dapper;
using Newtonsoft.Json;
using SimpleEventSourcing.Samples.Realtime;
using Microsoft.AspNetCore.SignalR;
using SimpleEventSourcing.CloudFirestore;
using SimpleEventSourcing.Samples.Providers;
using System.Threading.Tasks;
using SimpleEventSourcing.Samples.Events;
using Converto;
using static SimpleEventSourcing.Samples.Delivery.Configuration;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.Samples.Delivery
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    HostingEnvironment = builderContext.HostingEnvironment;
                })
                .ConfigureServices(s => s.AddSignalR())
                .ConfigureServices(s => s.AddRouting())
                .ConfigureServices(s => s.AddMvc())
                .ConfigureServices(s =>
                {
                    s.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new Info { Title = "Delivery API", Version = "v1" });
                        c.GenerateTagadaSwaggerDoc();
                    });
                })
                .ConfigureServices(s =>
                {
                    s.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowCredentials()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }));
                })
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.UseCors("CorsPolicy");
                    
                    HandleDatabaseCreation();

                    var dataProvider = new CloudFirestoreProvider("event-sourcing-da233", "firebase.json");
                    var streamProvider = new CloudFirestoreEventStreamProvider<StreamedEvent>(dataProvider.Database, new StreamedEventFirestoreConverter(), new EventStreamFirestoreConverter());

                    var eventStore = EventStoreBuilder<StreamedEvent>
                        .New()
                        .WithStreamProvider(streamProvider)
                        .WithApplyFunction(new ValidateOrderApplyFunction())
                        .WithApplyFunction(new CancelOrderApplyFunction())
                        .Build();

                    var projection = new OrderProjection(streamProvider);

                    app.Map("/api")
                        .Get("/all", GetAllOrders)
                        .Post<ValidateOrderCommand>("/validate", async (command) => await eventStore.ApplyAsync(command))
                        .Post<CancelOrderCommand>("/cancel", async (command) => await eventStore.ApplyAsync(command))
                        .AddSwagger()
                        .Use();

                    app.UseSignalR(routes =>
                    {
                        routes.MapHub<OrderHub>("/order");
                    });

                    projection.ObserveEntityChange().Subscribe(async entity =>
                    {
                        var hub = app.ApplicationServices.GetRequiredService<IHubContext<OrderHub>>();
                        await hub.Clients.All.SendAsync("Sync", entity);
                    });

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Delivery API V1");
                    });
                })
                .Build()
                .Run();
        }

        public static void HandleDatabaseCreation()
        {
            using (var connection = GetDatabaseConnection())
            {
                connection.Execute(
                    @"
                    CREATE TABLE IF NOT EXISTS [Order] (
                        [Id] VARCHAR(36) NOT NULL PRIMARY KEY,
                        [Number] INTEGER NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [IsConfirmed] INTEGER NOT NULL,
                        [IsCanceled] INTEGER NOT NULL,
                        [Items] TEXT NOT NULL
                    );
                    "
                );
            }
        }

        public static Func<GetOrdersQuery, IEnumerable<Order>> GetAllOrders = _ =>
        {
            using (var connection = GetDatabaseConnection())
            {
                var orders = connection
                    .Query<OrderDbo>("SELECT * FROM [Order]")
                    .ToList();

                return orders.Select(order =>
                {
                    return new Order
                    {
                        Id = order.Id,
                        CreatedDate = order.CreatedDate,
                        Number = order.Number,
                        IsConfirmed = order.IsConfirmed,
                        IsCanceled = order.IsCanceled,
                        Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(order.Items)
                    };
                });
            }
        };
    }

    public class OrderDbo
    {
        public string Id { get; set; }
        public long Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCanceled { get; set; }
        public string Items { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public long Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCanceled { get; set; }
        public IEnumerable<OrderedItem> Items { get; set; }
    }
    public class OrderedItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderHub : SyncEntityHub<Order> { }

    public class GetOrdersQuery { }

    public class ValidateOrderCommand
    {
        public string OrderId { get; set; }
    }

    public class CancelOrderCommand
    {
        public string OrderId { get; set; }
    }

    public class ValidateOrderApplyFunction : IApplyFunction<ValidateOrderCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(ValidateOrderCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<OrderValidated>();

            string streamId = $"order-{@event.OrderId}";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class CancelOrderApplyFunction : IApplyFunction<CancelOrderCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(CancelOrderCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<OrderCanceled>();

            string streamId = $"order-{@event.OrderId}";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
}