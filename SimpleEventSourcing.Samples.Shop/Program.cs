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
using SimpleEventSourcing.Samples.Realtime;
using Microsoft.AspNetCore.SignalR;
using SimpleEventSourcing.CloudFirestore;
using SimpleEventSourcing.Samples.Providers;
using System.Threading.Tasks;
using SimpleEventSourcing.Samples.Events;
using Converto;
using static SimpleEventSourcing.Samples.Shop.Configuration;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.Samples.Shop
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
                        c.SwaggerDoc("v1", new Info { Title = "Shop API", Version = "v1" });
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

                    var projection = new CartProjection(streamProvider);

                    var eventStore = EventStoreBuilder<StreamedEvent>
                        .New()
                        .WithStreamProvider(streamProvider)
                        .WithApplyFunction(new AddItemInCartApplyFunction())
                        .WithApplyFunction(new RemoveItemFromCartApplyFunction())
                        .WithApplyFunction(new ResetCartApplyFunction())
                        .WithApplyFunction(new CreateOrderFromCartApplyFunction(projection))
                        .Build();

                    app.Map("/api")
                        .Get<GetCartQuery, Cart>("/cart", _ => projection.GetCart())
                        .PostAsync<AddItemInCartCommand>("/cart/addItem", eventStore.ApplyAsync)
                        .PostAsync<RemoveItemFromCartCommand>("/cart/removeItem", eventStore.ApplyAsync)
                        .PostAsync<ResetCartCommand>("/cart/reset", eventStore.ApplyAsync)
                        .PostAsync<CreateOrderFromCartCommand>("/order", eventStore.ApplyAsync)
                        .AddSwagger()
                        .Use();

                    app.UseSignalR(routes =>
                    {
                        routes.MapHub<CartHub>("/cart");
                    });

                    projection.ObserveEntityChange().Subscribe(async entity =>
                    {
                        var hub = app.ApplicationServices.GetRequiredService<IHubContext<CartHub>>();
                        await hub.Clients.All.SendAsync("Sync", entity);
                    });

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1");
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
                    CREATE TABLE IF NOT EXISTS [Cart] (
                        [Id] VARCHAR(36) NOT NULL PRIMARY KEY,
                        [Quantity] INTEGER NOT NULL
                    );
                    "
                );
            }
        }
    }

    public class Cart
    {
        public IEnumerable<ItemAndQuantity> Items { get; set; }
    }
    public class ItemAndQuantity
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartHub : SyncEntityHub<ItemAndQuantity> { }

    public class GetCartQuery { }

    public class AddItemInCartCommand
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class RemoveItemFromCartCommand
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class ResetCartCommand { }

    public class CreateOrderFromCartCommand { }

    public class AddItemInCartApplyFunction : IApplyFunction<AddItemInCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(AddItemInCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<CartItemSelected>();

            string streamId = "cart";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class RemoveItemFromCartApplyFunction : IApplyFunction<RemoveItemFromCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(RemoveItemFromCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<CartItemUnselected>();

            string streamId = "cart";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class ResetCartApplyFunction : IApplyFunction<ResetCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(ResetCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<CartReseted>();

            string streamId = "cart";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class CreateOrderFromCartApplyFunction : IApplyFunction<CreateOrderFromCartCommand, StreamedEvent>
    {
        private readonly CartProjection _cartProjection;

        public CreateOrderFromCartApplyFunction(CartProjection cartProjection)
        {
            _cartProjection = cartProjection;
        }

        public async Task ExecuteAsync(CreateOrderFromCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = new OrderedFromCart
            {
                Items = _cartProjection.GetCart().Items
                    .Select(item => new OrderedFromCart.OrderedItem { ItemId = item.ItemId, Quantity = item.Quantity })
                    .ToList()
            };

            string streamId = "cart";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
}