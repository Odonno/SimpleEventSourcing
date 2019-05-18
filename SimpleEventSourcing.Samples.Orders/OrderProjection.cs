using Converto;
using Dapper;
using Newtonsoft.Json;
using SimpleEventSourcing.Samples.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using static SimpleEventSourcing.Samples.Orders.Configuration;

namespace SimpleEventSourcing.Samples.Orders
{
    public class OrderProjection : Projection<StreamedEvent>
    {
        private readonly Subject<Order> _updatedEntitySubject = new Subject<Order>();

        public OrderProjection(IEventStreamProvider<StreamedEvent> streamProvider) : base(streamProvider)
        {
            const string searchedStreams = "order-";

            // Detect new events from existing streams
            var newEventsFromExistingStreams = streamProvider
                .GetAllStreamsAsync()
                .ToObservable()
                .SelectMany(stream => stream)
                .Where(stream => stream.Id.StartsWith(searchedStreams))
                .SelectMany(stream => stream.ListenForNewEvents(false));

            // Detect new events from new streams
            var newEventsFromNewStreams = streamProvider
                .ListenForNewStreams()
                .Where(stream => stream.Id.StartsWith(searchedStreams))
                .SelectMany(stream => stream.ListenForNewEvents(true));

            // Handle new events
            Observable.Merge(newEventsFromExistingStreams, newEventsFromNewStreams)
                .Subscribe(@event => Handle(@event));
        }

        public IObservable<Order> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(StreamedEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(OrderedFromCart))
            {
                var data = @event.Data.ConvertTo<OrderedFromCart>();
                var metadata = @event.Metadata.ConvertTo<StreamedEventMetadata>();

                using (var connection = GetDatabaseConnection())
                {
                    var newOrder = connection.Query<OrderDbo>(
                        @"
                        INSERT INTO [Order] 
                        ([Number], [CreatedDate], [IsConfirmed], [IsCanceled], [Items])
                        VALUES ((SELECT IFNULL(MAX([Number]), 0) + 1 FROM [Order]), @CreatedDate, 0, 0, @Items);

                        SELECT * FROM [Order] ORDER BY [Id] DESC LIMIT 1;
                        ",
                        new {
                            metadata.CreatedAt,
                            Items = JsonConvert.SerializeObject(new List<OrderedItem>()) // TODO
                        }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = newOrder.Id,
                            CreatedDate = newOrder.CreatedDate,
                            Number = newOrder.Number,
                            IsConfirmed = newOrder.IsConfirmed,
                            IsCanceled = newOrder.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(newOrder.Items)
                        });
                    }
                }
            }
            if (@event.EventName == nameof(OrderValidated))
            {
                var data = @event.Data.ConvertTo<OrderValidated>();

                using (var connection = GetDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsConfirmed] = 1
                        WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(order.Items)
                        });
                    }
                }
            }
            if (@event.EventName == nameof(OrderCanceled))
            {
                var data = @event.Data.ConvertTo<OrderCanceled>();

                using (var connection = GetDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsCanceled] = 1
                        WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(order.Items)
                        });
                    }
                }
            }
        }
    }
}
