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
using static SimpleEventSourcing.Samples.Shop.Configuration;

namespace SimpleEventSourcing.Samples.Shop
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static readonly AppCommandDispatcher AppCommandDispatcher = new AppCommandDispatcher();
        public static readonly AppEventStore AppEventStore = new AppEventStore(AppCommandDispatcher.ObserveEventAggregate());
        public static readonly CartEventView CartEventView = new CartEventView(AppEventStore.ObserveEvent());

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
                        c.SwaggerDoc("v1", new Info { Title = "Shop API", Version = "v1" });
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
                        .Get("/cart", GetCart)
                        .Post<AddItemInCartCommand>("/cart/addItem", AppCommandDispatcher.Dispatch)
                        .Post<RemoveItemFromCartCommand>("/cart/removeItem", AppCommandDispatcher.Dispatch)
                        .Post<ResetCartCommand>("/cart/reset", AppCommandDispatcher.Dispatch)
                        .Post<CreateOrderFromCartCommand>("/order", AppCommandDispatcher.Dispatch)
                        .AddSwagger()
                        .Use();

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
                        [ItemId] INTEGER NOT NULL,
                        [Quantity] INTEGER NOT NULL
                    );
                    "
                );
            }
        }

        public static Func<GetCartQuery, Cart> GetCart = _ =>
        {
            using (var connection = GetDatabaseConnection())
            {
                return new Cart
                {
                    Items = connection
                        .Query<ItemAndQuantity>("SELECT [ItemId], [Quantity] FROM [Cart]")
                        .ToList()
                };
            }
        };
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

    public class GetCartQuery { }

    public class AddItemInCartCommand
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemFromCartCommand
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class ResetCartCommand { }

    public class CreateOrderFromCartCommand { }
}