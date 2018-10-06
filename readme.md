# Simple Event Sourcing

> Event Sourcing made simple using Reactive Extensions

Simple Event Sourcing is a .NET library based on the Event Sourcing architecture. This library is written with Rx.NET and built with the *minimum of code* you need to create event sourced application.

## Example app

There is a sample application in this repository. It is a console application that showcases how you can use this library in your backend application. The console application focuses on:

* A list of events on a concrete domain (e-commerce cart)
* An in-memory aggregator (building a State in-memory)
* A relational database aggregator (using sqlite database system)

## Getting started

Simple Event Sourcing library is defined by 3 different components:

* Events - a list of possible actions with the system (ex: user orders a book)
* Event Store - the recording of events & the single source of truth (events stored in-memory or in a database)
* Event View - a point of view extracted from the events (ex: total sales per user)

### Events

Events is your own definition of the business actions. Using this library, you have to express each event as `class` and we take care of the rest. Here are some examples of events:

```csharp
public class AddItemInCartEvent
{
    public string ItemName { get; set; }
    public int NumberOfUnits { get; set; } = 1;
}

public class RemoveItemFromCartEvent
{
    public string ItemName { get; set; }
}

public class ResetCartEvent { }
```

### Write Model - Event Store

Using Event Sourcing, the Write model consists of a set of events which are stored in the commonly named Event Store.

The `EventStore` class should be redefine for your purpose. By default, the behavior of the `EventStore` is to store events in-memory.

```csharp
public abstract class EventStore
{
    public virtual void Dispatch(object @event);

    public IObservable<object> ObserveEvent();
    public IObservable<T> ObserveEvent<T>();
}
```

If you need to make a persistent `EventStore`, feel free to override the `Dispatch` and store events in a database system.

### Read Model - Event View

Using Event Sourcing, the Read model consists of reading a set of events from the Event Store to get a consistent view of the business.

Using this library, you will have the freedom to choose between two kind of views:

1. In-memory view - listening to events will generate a view used instantly
2. Persistent view - listening to events will create data stored in a database system

#### In-memory view

```csharp
public abstract class InMemoryEventView<TState> where TState : class, new()
{
    public TState State { get; }

    protected InMemoryEventView(IObservable<object> events, TState initialState = null) { }

    public IObservable<TState> ObserveState();
    public IObservable<TPartial> ObserveState<TPartial>(Func<TState, TPartial> selector);
}
```

In this particular form of `EventView`, from an event you get a new state immediately available for consumption/subscription. This form of `EventView` is close to the Redux pattern.

#### Persistent view

This other form of `EventView` gives you the ability to override the `Handle` method in which you can update a specific part of your data like a Table in a relational database.

```csharp
public abstract class EventView
{
    protected EventView(IObservable<object> events) { }

    public virtual void Replay(object @event);
    public virtual void Replay(IEnumerable<object> events);
}
```

Notice that we offer a `Replay` mechanism you can use to replay the history in case you changed the logic of the handler or the structure of the database.
