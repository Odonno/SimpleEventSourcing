﻿using Microsoft.AspNetCore;
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
using System.Threading.Tasks;
using Converto;
using SimpleEventSourcing.Samples.Events;
using SimpleEventSourcing.Samples.Providers;
using static SimpleEventSourcing.Samples.Inventory.Configuration;
using static System.Guid;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.Samples.Inventory
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
                        c.SwaggerDoc("v1", new Info { Title = "Inventory API", Version = "v1" });
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
                    var cloudFirestoreStreamProvider = new CloudFirestoreEventStreamProvider<StreamedEvent>(dataProvider.Database, new StreamedEventFirestoreConverter(), new EventStreamFirestoreConverter());

                    var projection = new ItemProjection(cloudFirestoreStreamProvider);

                    var eventStore = EventStoreBuilder<StreamedEvent>
                        .New()
                        .WithStreamProvider(cloudFirestoreStreamProvider)
                        .WithApplyFunction(new CreateItemApplyFunction())
                        .WithApplyFunction(new UpdateItemPriceApplyFunction())
                        .WithApplyFunction(new SupplyItemApplyFunction())
                        .WithEvolveFunction(new ShipItemEvolveFunction())
                        .Build();

                    app.Map("/api")
                        .Get("/all", projection.GetAllItems)
                        .PostAsync<CreateItemCommand>("/create", eventStore.ApplyAsync)
                        .PostAsync<UpdateItemPriceCommand>("/updatePrice", eventStore.ApplyAsync)
                        .PostAsync<SupplyItemCommand>("/supply", eventStore.ApplyAsync)
                        .AddSwagger()
                        .Use();

                    app.UseSignalR(routes =>
                    {
                        routes.MapHub<ItemHub>("/item");
                    });

                    projection.ObserveEntityChange().Subscribe(async entity =>
                    {
                        var hub = app.ApplicationServices.GetRequiredService<IHubContext<ItemHub>>();
                        await hub.Clients.All.SendAsync("Sync", entity);
                    });

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
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
                    CREATE TABLE IF NOT EXISTS [Item] (
                        [Id] VARCHAR(36) NOT NULL PRIMARY KEY,
                        [Name] VARCHAR(200) NOT NULL,
                        [Price] DECIMAL(12, 2) NOT NULL,
                        [RemainingQuantity] INTEGER NOT NULL
                    );
                    "
                );
            }
        }
    }

    public class ItemDbo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class ItemHub : SyncEntityHub<Item> { }

    public class GetItemsQuery { }

    public class CreateItemCommand
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public long InitialQuantity { get; set; }
    }

    public class UpdateItemPriceCommand
    {
        public string ItemId { get; set; }
        public double NewPrice { get; set; }
    }

    public class SupplyItemCommand
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class CreateItemApplyFunction : IApplyFunction<CreateItemCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(CreateItemCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            // TODO : Create ConvertWith function
            var @event = command.ConvertTo<ItemRegistered>().With(new { Id = NewGuid().ToString() });

            string streamId = $"item-{@event.Id}";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class UpdateItemPriceApplyFunction : IApplyFunction<UpdateItemPriceCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(UpdateItemPriceCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<ItemPriceUpdated>();

            string streamId = $"item-{@event.ItemId}";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }
    public class SupplyItemApplyFunction : IApplyFunction<SupplyItemCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(SupplyItemCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<ItemSupplied>();

            string streamId = $"item-{@event.ItemId}";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
        }
    }

    public class ShipItemEvolveFunction : IEvolveFunction<StreamedEvent>
    {
        public bool OfEvent(StreamedEvent @event)
        {
            return @event.EventName == nameof(OrderValidated);
        }
        public Task<bool> ShouldListenStreamsAsync(string streamId)
        {
            return Task.FromResult(streamId.StartsWith("order-"));
        }
        public async Task ExecuteAsync(StreamedEvent @event, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var data = @event.Data as OrderValidated;
            var orderStream = await eventStreamProvider.GetStreamAsync($"order-{data.OrderId}");

            var orderCreatedEvent = await orderStream.GetEventAsync(1);
            var orderCreatedData = orderCreatedEvent.Data as OrderCreated;

            foreach (var item in orderCreatedData.Items)
            {
                var shippedItemEvent = new ItemShipped
                {
                    OrderId = data.OrderId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity
                };

                string streamId = $"item-{item.ItemId}";
                var stream = await eventStreamProvider.GetStreamAsync(streamId);
                var currentPosition = await stream.GetCurrentPositionAsync();

                var events = List(shippedItemEvent);
                await stream.AppendEventsAsync(CreateStreamedEvents(streamId, currentPosition, events));
            }
        }
    }
}