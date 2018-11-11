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
    public class ItemController : Controller
    {
        [HttpGet("all")]
        public IEnumerable<Item> GetAll()
        {
            using (var connection = GetViewsDatabaseConnection())
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
        }

        [HttpPost("create")]
        public void CreateItem(CreateItemRequest request)
        {
            AppCommandDispatcher.Dispatch(new CreateItemCommand
            {
                Name = request.Name,
                Price = request.Price,
                InitialQuantity = request.InitialQuantity
            });
        }

        [HttpPost("updatePrice")]
        public void UpdatePrice(UpdateItemPriceRequest request)
        {
            AppCommandDispatcher.Dispatch(new UpdateItemPriceCommand
            {
                ItemId = request.ItemId,
                NewPrice = request.NewPrice
            });
        }

        [HttpPost("supply")]
        public void Supply(SupplyItemRequest request)
        {
            AppCommandDispatcher.Dispatch(new SupplyItemCommand
            {
                ItemId = request.ItemId,
                Quantity = request.Quantity
            });
        }
    }

    public class CreateItemRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class UpdateItemPriceRequest
    {
        public long ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class SupplyItemRequest
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
