using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using static SimpleEventSourcing.Samples.Web.Program;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;
using System;

namespace SimpleEventSourcing.Samples.Web.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        [HttpGet("all")]
        public IEnumerable<Order> GetAll()
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                var orders = connection
                    .Query<OrderDbo>("SELECT * FROM [Order]")
                    .ToList();
                var itemsOrdered = connection
                    .Query<ItemOrderedDbo>("SELECT * FROM [ItemOrdered]")
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
                        Items = itemsOrdered
                            .Where(i => i.OrderId == order.Id)
                            .Select(i =>
                            {
                                return new ItemAndPriceAndQuantity
                                {
                                    ItemId = i.ItemId,
                                    Price = Convert.ToDecimal(i.Price),
                                    Quantity = i.Quantity
                                };
                            })
                    };
                });
            }
        }

        [HttpPost("validate")]
        public void Validate(ValidateOrderRequest request)
        {
            AppCommandDispatcher.Dispatch(new ValidateOrderCommand
            {
                OrderId = request.OrderId
            });
        }

        [HttpPost("cancel")]
        public void Cancel(CancelOrderRequest request)
        {
            AppCommandDispatcher.Dispatch(new CancelOrderCommand
            {
                OrderId = request.OrderId
            });
        }
    }

    public class ValidateOrderRequest
    {
        public long OrderId { get; set; }
    }

    public class CancelOrderRequest
    {
        public long OrderId { get; set; }
    }
}
