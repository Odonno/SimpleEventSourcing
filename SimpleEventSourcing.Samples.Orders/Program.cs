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
using static SimpleEventSourcing.Samples.Orders.Configuration;
using Newtonsoft.Json;

namespace SimpleEventSourcing.Samples.Orders
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static readonly AppCommandDispatcher AppCommandDispatcher = new AppCommandDispatcher();
        public static readonly AppEventStore AppEventStore = new AppEventStore(AppCommandDispatcher.ObserveEventAggregate());
        public static readonly OrderEventView OrderEventView = new OrderEventView(AppEventStore.ObserveEvent());

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
                        c.SwaggerDoc("v1", new Info { Title = "Orders API", Version = "v1" });
                        c.GenerateTagadaSwaggerDoc();
                    });
                })
                .ConfigureServices(s =>
                {
                    s.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.AllowAnyOrigin()
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

                    app.Map("/api")
                        .Get("/all", GetAllOrders)
                        .Post<ValidateOrderCommand>("/validate", AppCommandDispatcher.Dispatch)
                        .Post<CancelOrderCommand>("/cancel", AppCommandDispatcher.Dispatch)
                        .AddSwagger()
                        .Use();

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API V1");
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
                //var itemsOrdered = connection
                //    .Query<ItemOrderedDbo>("SELECT * FROM [ItemOrdered]")
                //    .ToList();

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
                        //Items = itemsOrdered
                        //    .Where(i => i.OrderId == order.Id)
                        //    .Select(i =>
                        //    {
                        //        return new ItemAndPriceAndQuantity
                        //        {
                        //            ItemId = i.ItemId,
                        //            Price = Convert.ToDecimal(i.Price),
                        //            Quantity = i.Quantity
                        //        };
                        //    })
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

    public class GetOrdersQuery { }

    public class ValidateOrderCommand
    {
        public string OrderId { get; set; }
    }

    public class CancelOrderCommand
    {
        public string OrderId { get; set; }
    }
}