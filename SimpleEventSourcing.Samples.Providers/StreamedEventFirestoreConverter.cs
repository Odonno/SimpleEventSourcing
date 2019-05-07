using Google.Cloud.Firestore;
using SimpleEventSourcing.CloudFirestore;
using SimpleEventSourcing.Samples.Events;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Samples.Providers
{
    public class StreamedEventFirestoreConverter : ICloudFirestoreEventConverter<StreamedEvent>
    {
        public StreamedEvent FromFirestore(DocumentSnapshot documentSnapshot)
        {
            var eventDictionary = documentSnapshot.ToDictionary();
            string eventName = eventDictionary["eventName"] as string;

            var dataDictionary = eventDictionary["data"] as Dictionary<string, object>;
            var metadataDictionary = eventDictionary["metadata"] as Dictionary<string, object>;

            return new StreamedEvent
            {
                StreamId = eventDictionary["streamId"] as string,
                Position = (long)eventDictionary["position"],
                Id = eventDictionary["id"] as string,
                EventName = eventName,
                Data = GetEventDataFromFirestore(eventName, dataDictionary),
                Metadata = new StreamedEventMetadata
                {
                    CreatedAt = ((Timestamp)metadataDictionary["createdAt"]).ToDateTime(),
                    CorrelationId = metadataDictionary["correlationId"] as string
                }
            };
        }

        private object GetEventDataFromFirestore(string eventName, Dictionary<string, object> dataDictionary)
        {
            if (eventName == nameof(ItemRegistered))
            {
                return new ItemRegistered
                {
                    Id = dataDictionary["id"] as string,
                    Name = dataDictionary["name"] as string,
                    Price = (double)dataDictionary["price"],
                    InitialQuantity = (long)dataDictionary["initialQuantity"]
                };
            }
            if (eventName == nameof(ItemPriceUpdated))
            {
                return new ItemPriceUpdated
                {
                    ItemId = dataDictionary["itemId"] as string,
                    NewPrice = (double)dataDictionary["newPrice"]
                };
            }
            if (eventName == nameof(ItemSupplied))
            {
                return new ItemSupplied
                {
                    ItemId = dataDictionary["itemId"] as string,
                    Quantity = (long)dataDictionary["quantity"]
                };
            }
            if (eventName == nameof(ItemReserved))
            {
                return new ItemReserved
                {
                    ItemId = dataDictionary["itemId"] as string,
                    OrderId = dataDictionary["orderId"] as string,
                    Quantity = (long)dataDictionary["quantity"]
                };
            }
            if (eventName == nameof(ItemShipped))
            {
                return new ItemShipped
                {
                    ItemId = dataDictionary["itemId"] as string,
                    OrderId = dataDictionary["orderId"] as string,
                    Quantity = (long)dataDictionary["quantity"]
                };
            }

            if (eventName == nameof(OrderCreated))
            {
                var itemsDictionaries = dataDictionary["items"] as List<Dictionary<string, object>>;
                return new OrderCreated
                {
                    Id = dataDictionary["id"] as string,
                    Items = new List<OrderCreated.OrderedItem>(
                        itemsDictionaries.Select(d =>
                        {
                            return new OrderCreated.OrderedItem
                            {
                                ItemId = d["itemId"] as string,
                                Quantity = (long)d["quantity"]
                            };
                        })
                    )
                };
            }
            if (eventName == nameof(OrderValidated))
            {
                return new OrderValidated
                {
                    OrderId = dataDictionary["orderId"] as string
                };
            }
            if (eventName == nameof(OrderCanceled))
            {
                return new OrderCanceled
                {
                    OrderId = dataDictionary["orderId"] as string
                };
            }

            if (eventName == nameof(CartItemSelected))
            {
                return new CartItemSelected
                {
                    ItemId = dataDictionary["itemId"] as string,
                    Quantity = (long)dataDictionary["quantity"]
                };
            }
            if (eventName == nameof(CartItemUnselected))
            {
                return new CartItemUnselected
                {
                    ItemId = dataDictionary["itemId"] as string,
                    Quantity = (long)dataDictionary["quantity"]
                };
            }
            if (eventName == nameof(CartReseted))
            {
                return new CartReseted();
            }
            if (eventName == nameof(OrderedFromCart))
            {
                return new OrderedFromCart();
            }

            return null;
        }

        public object ToFirestore(StreamedEvent @event)
        {
            return new Dictionary<string, object>
            {
                { "streamId", @event.StreamId },
                { "position", @event.Position },
                { "id", @event.Id },
                { "eventName", @event.EventName },
                { "data", EventDataToFirestore(@event.Data) },
                { "metadata", EventMetadataToFirestore(@event.Metadata) }
            };
        }

        private Dictionary<string, object> EventDataToFirestore(object data)
        {
            if (data is ItemRegistered itemRegistered)
            {
                return new Dictionary<string, object>
                {
                    { "id", itemRegistered.Id },
                    { "name", itemRegistered.Name },
                    { "price", itemRegistered.Price },
                    { "initialQuantity", itemRegistered.InitialQuantity }
                };
            }
            if (data is ItemPriceUpdated itemPriceUpdated)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", itemPriceUpdated.ItemId },
                    { "newPrice", itemPriceUpdated.NewPrice }
                };
            }
            if (data is ItemSupplied itemSupplied)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", itemSupplied.ItemId },
                    { "quantity", itemSupplied.Quantity }
                };
            }
            if (data is ItemReserved itemReserved)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", itemReserved.ItemId },
                    { "orderId", itemReserved.OrderId },
                    { "quantity", itemReserved.Quantity }
                };
            }
            if (data is ItemShipped itemShipped)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", itemShipped.ItemId },
                    { "orderId", itemShipped.OrderId },
                    { "quantity", itemShipped.Quantity }
                };
            }

            if (data is OrderCreated orderCreated)
            {
                return new Dictionary<string, object>
                {
                    { "id", orderCreated.Id },
                    {
                        "items",
                        orderCreated.Items.Select(i => 
                        {
                            return new Dictionary<string, object>
                            {
                                { "itemId", i.ItemId },
                                { "quantity", i.Quantity }
                            };
                        })
                    }
                };
            }
            if (data is OrderValidated orderValidated)
            {
                return new Dictionary<string, object>
                {
                    { "orderId", orderValidated.OrderId }
                };
            }
            if (data is OrderCanceled orderCanceled)
            {
                return new Dictionary<string, object>
                {
                    { "orderId", orderCanceled.OrderId }
                };
            }

            if (data is CartItemSelected cartItemSelected)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", cartItemSelected.ItemId },
                    { "quantity", cartItemSelected.Quantity }
                };
            }
            if (data is CartItemUnselected cartItemUnselected)
            {
                return new Dictionary<string, object>
                {
                    { "itemId", cartItemUnselected.ItemId },
                    { "quantity", cartItemUnselected.Quantity }
                };
            }
            if (data is CartReseted)
            {
                return new Dictionary<string, object>();
            }
            if (data is OrderedFromCart)
            {
                return new Dictionary<string, object>();
            }
            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> EventMetadataToFirestore(StreamedEventMetadata metadata)
        {
            return new Dictionary<string, object>
            {
                { "createdAt", metadata.CreatedAt },
                { "correlationId", metadata.CorrelationId }
            };
        }
    }
}
