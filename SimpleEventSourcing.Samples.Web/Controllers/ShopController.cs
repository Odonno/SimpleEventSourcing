using Microsoft.AspNetCore.Mvc;
using System;
using Dapper;
using System.Linq;
using static SimpleEventSourcing.Samples.Web.Program;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

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
            AppEventStore.Dispatch(new AddItemInCartEvent
            {
                ItemId = request.ItemId,
                Quantity = request.Quantity
            });
        }

        [HttpPost("cart/removeItem")]
        public void RemoveItemFromCart(RemoveItemFromCartRequest request)
        {
            AppEventStore.Dispatch(new RemoveItemFromCartEvent
            {
                ItemId = request.ItemId,
                Quantity = request.Quantity
            });
        }

        [HttpPost("cart/reset")]
        public void ResetCart()
        {
            AppEventStore.Dispatch(new ResetCartEvent());
        }

        [HttpPost("order")]
        public void Order(CreateOrderFromCartRequest request)
        {
            AppEventStore.Dispatch(new CreateOrderFromCartEvent { Date = request.Date });
            AppEventStore.Dispatch(new ResetCartEvent());
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

    public class CreateOrderFromCartRequest
    {
        public DateTime Date { get; set; }
    }
}
