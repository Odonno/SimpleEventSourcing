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
using SimpleEventSourcing.Samples.Events;
using static SimpleEventSourcing.Samples.Inventory.Configuration;

namespace SimpleEventSourcing.Samples.Inventory
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static readonly AppCommandDispatcher AppCommandDispatcher = new AppCommandDispatcher();
        public static readonly AppEventStore AppEventStore = new AppEventStore(AppCommandDispatcher.ObserveEventAggregate());
        public static readonly ItemEventView ItemEventView = new ItemEventView(AppEventStore.ObserveEvent());

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    HostingEnvironment = builderContext.HostingEnvironment;
                })
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
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    HandleDatabaseCreation();

                    app.Map("/api")
                        .Get("/all", GetAllItems)
                        .Post<CreateItemCommand>("/create", AppCommandDispatcher.Dispatch)
                        .Post<UpdateItemPriceCommand>("/updatePrice", AppCommandDispatcher.Dispatch)
                        .Post<SupplyItemCommand>("/supply", AppCommandDispatcher.Dispatch)
                        .AddSwagger()
                        .Use();

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

        public static Func<GetItemsQuery, IEnumerable<Item>> GetAllItems = _ =>
        {
            using (var connection = GetDatabaseConnection())
            {
                return connection
                    .Query<ItemDbo>("SELECT * FROM [Item]")
                    .Select(item =>
                    {
                        return new Item
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Price = Convert.ToDecimal(item.Price),
                            RemainingQuantity = item.RemainingQuantity,
                        };
                    })
                    .ToList();
            }
        };
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

    public class GetItemsQuery { }

    public class CreateItemCommand
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class UpdateItemPriceCommand
    {
        public string ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class SupplyItemCommand
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}