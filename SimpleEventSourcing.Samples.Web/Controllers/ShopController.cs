using Microsoft.AspNetCore.Mvc;
using System;
using Dapper;
using System.Linq;
using static SimpleEventSourcing.Samples.Web.Program;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;

namespace SimpleEventSourcing.Samples.Web.Controllers
{
    [Route("api/[controller]")]
    public class ShopController : Controller
    {
        [HttpGet("cart")]
        public Cart GetCart()
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                return new Cart
                {
                    Items = connection
                        .Query<ItemAndQuantity>("SELECT [ItemId], [Quantity] FROM [Cart]")
                        .ToList()
                };
            }
        }

        [HttpPost("cart/addItem")]
        public void AddItemInCart(AddItemInCartRequest request)
        {
            AppCommandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemId = request.ItemId,
                Quantity = request.Quantity
            });
        }

        [HttpPost("cart/removeItem")]
        public void RemoveItemFromCart(RemoveItemFromCartRequest request)
        {
            AppCommandDispatcher.Dispatch(new RemoveItemFromCartCommand
            {
                ItemId = request.ItemId,
                Quantity = request.Quantity
            });
        }

        [HttpPost("cart/reset")]
        public void ResetCart()
        {
            AppCommandDispatcher.Dispatch(new ResetCartCommand());
        }

        [HttpPost("order")]
        public void Order()
        {
            AppCommandDispatcher.Dispatch(new CreateOrderFromCartCommand());
        }
    }

    public class AddItemInCartRequest
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemFromCartRequest
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
